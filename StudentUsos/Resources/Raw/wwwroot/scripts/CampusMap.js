

window.addEventListener(
    "HybridWebViewMessageReceived",
    function (e) {
        console.log(e);

    });

function ReceiveFloorData(jsonString) {

    console.log(jsonString);

    json = JSON.parse(jsonString);

    roomFillContainerId = "rooms"
    roomLabelContainerId = "labels"

    svg = document.querySelector("svg")
    roomLabelContainer = document.createElementNS("http://www.w3.org/2000/svg", "g")
    roomLabelContainer.setAttributeNS(null, "id", roomLabelContainerId)
    //svg.insertBefore(roomLabelContainer, svg.children[0])
    svg.appendChild(roomLabelContainer)

    container = document.getElementById(roomFillContainerId)
    children = container.children

    for (let index = 0; index < children.length; index++) {
        box = children[index].getBBox()

        let localCenter = calculateCenterWithCentroidPlusNormals(children[index]);
        let point = svg.createSVGPoint();
        point.x = localCenter[0];
        point.y = localCenter[1];
        let globalPoint = point.matrixTransform(children[index].getCTM());
        let inverseMatrix = roomLabelContainer.getCTM().inverse();
        let adjustedPoint = globalPoint.matrixTransform(inverseMatrix);

        let xCenter = adjustedPoint.x;
        let yCenter = adjustedPoint.y;

        text = document.createElementNS("http://www.w3.org/2000/svg", "text")
        text.setAttributeNS(null, "class", "roomLabel")
        text.setAttributeNS(null, "dominant-baseline", "middle");
        text.setAttributeNS(null, "text-anchor", "middle");
        text.setAttributeNS(null, "x", xCenter);
        text.setAttributeNS(null, "y", yCenter);
        text.setAttributeNS(null, "style", "font-size: 15px;")

        roomId = children[index].getAttribute("roomid");
        text.appendChild(document.createTextNode(roomIdToName(json, roomId)))

        roomLabelContainer.appendChild(text)

        textRect1 = text.getBBox()

        const fontGrowthRate = 15;
        const roomBoundingBoxMultiplier = 0.3;
        fontSize = fontGrowthRate * box.width * roomBoundingBoxMultiplier / textRect1.width
        minFontSize = 10
        maxFontSize = 20
        if (fontSize < minFontSize) fontSize = minFontSize
        if (fontSize > maxFontSize) fontSize = maxFontSize
        text.setAttributeNS(null, "style", "font-size: " + fontSize + "px;")
    }

    assignOnClickEvents(children);

    centerScroll();
}

function centerScroll() {
    const svg = document.querySelector('svg');
    if (svg == null) {
        return;
    }
    const bbox = svg.getBoundingClientRect();

    const margin = parseFloat(window.getComputedStyle(svg).margin);

    const centerX = margin + bbox.width / 2;
    const centerY = margin + bbox.height / 2;

    window.scrollTo({
        left: centerX - window.innerWidth / 2,
        top: centerY - window.innerHeight / 2
    })
}

function assignOnClickEvents(rooms) {
    for (i = 0; i < rooms.length; i++) {
        rooms[i].addEventListener("click", async function (event) {
            const clickedElement = event.target;
            roomId = clickedElement.getAttribute("roomid");
            console.log(roomId);
            await window.HybridWebView.InvokeDotNet('ReceiveRoomClicked', [roomId]);
        });
    }
}

function ReceiveFloorSvg(svg) {
    body = document.getElementsByTagName("body")[0];
    body.innerHTML = svg;
    centerScroll();
}

function ReceiveCampusSvg(svg) {
    body = document.getElementsByTagName("body")[0];
    body.innerHTML = svg;
    centerScroll();

    campusBuildings = document.getElementById("pomieszczenia").children;
    for (i = 0; i < campusBuildings.length; i++) {
        campusBuildings[i].addEventListener("click", async function (event) {
            const clickedElement = event.target;
            roomId = clickedElement.getAttribute("id");
            roomId = roomId.replace("warta_", "");
            console.log(roomId);
            await window.HybridWebView.InvokeDotNet('ReceiveCampusBuildingClicked', [roomId]);
        });
    }
}

function roomIdToName(parsedJson, id) {
    names = [];
    records = [];
    for (let i = 0; i < parsedJson.length; i++) {
        if (parsedJson[i].roomId == id) {
            records.push(parsedJson[i]);
        }
    }
    if (records.length == 0) {
        return "";
    }
    names = records.sort((a, b) => b.nameWeight - a.nameWeight).map(x => x.name);
    return names.join(",");
}

function calculateCenterWithCentroidPlusNormals(svgPath) {
    const length = svgPath.getTotalLength();
    const step = 5;
    const points = [];

    for (let l = 0; l <= length; l += step) {
        const pt = svgPath.getPointAtLength(l);
        points.push({ x: pt.x, y: pt.y });
    }

    if (
        points.length > 0 &&
        (points[0].x !== points[points.length - 1].x ||
            points[0].y !== points[points.length - 1].y)
    ) {
        points.push({ ...points[0] });
    }

    // Centroid
    let signedArea = 0, cx = 0, cy = 0;
    for (let i = 0; i < points.length - 1; i++) {
        const x0 = points[i].x, y0 = points[i].y;
        const x1 = points[i + 1].x, y1 = points[i + 1].y;
        const a = x0 * y1 - x1 * y0;
        signedArea += a;
        cx += (x0 + x1) * a;
        cy += (y0 + y1) * a;
    }

    signedArea *= 0.5;
    cx /= (6 * signedArea);
    cy /= (6 * signedArea);

    let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
    for (let p of points) {
        minX = Math.min(minX, p.x);
        maxX = Math.max(maxX, p.x);
        minY = Math.min(minY, p.y);
        maxY = Math.max(maxY, p.y);
    }
    const bboxCx = (minX + maxX) / 2;
    const bboxCy = (minY + maxY) / 2;

    const dxToBoxCenter = cx - bboxCx;
    const dyToBoxCenter = cy - bboxCy;
    const offsetMagnitude = Math.hypot(dxToBoxCenter, dyToBoxCenter);

    // If the centroid is already near geometric center, skip adjustment
    const skewThreshold = Math.min(maxX - minX, maxY - minY) * 0.08;
    const isSkewed = offsetMagnitude > skewThreshold;

    if (isSkewed == false) {
        return [cx, cy];
    }

    let normalX = 0, normalY = 0;
    for (let i = 0; i < points.length - 1; i++) {
        const p1 = points[i];
        const p2 = points[i + 1];
        const dx = p2.x - p1.x;
        const dy = p2.y - p1.y;

        const nx = -dy;
        const ny = dx;

        const vx = p1.x - cx;
        const vy = p1.y - cy;

        const dot = nx * vx + ny * vy;
        const dir = dot < 0 ? -1 : 1;

        normalX += nx * dir;
        normalY += ny * dir;
    }

    const len = Math.hypot(normalX, normalY);
    if (len > 0) {
        normalX /= len;
        normalY /= len;
    }

    const offset = Math.min(maxX - minX, maxY - minY) * 0.1;
    const adjustedX = cx + normalX * offset;
    const adjustedY = cy + normalY * offset;

    return [adjustedX, adjustedY];
}
function calculateCenterWithCentroid(svgPath) {
    const length = svgPath.getTotalLength();
    const step = 5;
    const points = [];

    for (let l = 0; l <= length; l += step) {
        const pt = svgPath.getPointAtLength(l);
        points.push({ x: pt.x, y: pt.y });
    }

    if (points.length > 0 &&
        (points[0].x !== points[points.length - 1].x ||
            points[0].y !== points[points.length - 1].y)) {
        points.push({ ...points[0] });
    }

    let signedArea = 0;
    let cx = 0;
    let cy = 0;

    for (let i = 0; i < points.length - 1; i++) {
        const x0 = points[i].x;
        const y0 = points[i].y;
        const x1 = points[i + 1].x;
        const y1 = points[i + 1].y;

        const a = x0 * y1 - x1 * y0;
        signedArea += a;
        cx += (x0 + x1) * a;
        cy += (y0 + y1) * a;
    }

    signedArea *= 0.5;
    cx /= (6 * signedArea);
    cy /= (6 * signedArea);

    return [cx, cy];
}