window.addEventListener(
    "HybridWebViewMessageReceived",
    function (e) {
        console.log(e);

    });

function ReceiveFloorData(jsonString) {
    const json = JSON.parse(jsonString);

    const roomFillContainerId = "rooms";
    const roomLabelContainerId = "labels";

    const svg = document.querySelector("svg");

    // Remove existing labels if present
    const existingLabels = document.getElementById(roomLabelContainerId);
    if (existingLabels) {
        svg.removeChild(existingLabels);
    }

    // Create new label container
    const roomLabelContainer = document.createElementNS("http://www.w3.org/2000/svg", "g");
    roomLabelContainer.setAttributeNS(null, "id", roomLabelContainerId);
    svg.appendChild(roomLabelContainer);

    const container = document.getElementById(roomFillContainerId);
    const children = container.children;

    for (let index = 0; index < children.length; index++) {
        const child = children[index];
        const box = child.getBBox();

        // Compute label center
        const localCenter = calculateCenterWithCentroidPlusNormals(child);
        const point = svg.createSVGPoint();
        point.x = localCenter[0];
        point.y = localCenter[1];

        const globalPoint = point.matrixTransform(child.getCTM());
        const adjustedPoint = globalPoint.matrixTransform(roomLabelContainer.getCTM().inverse());

        const xCenter = adjustedPoint.x;
        const yCenter = adjustedPoint.y;

        // Create text element
        const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
        text.setAttributeNS(null, "class", "roomLabel");
        text.setAttributeNS(null, "dominant-baseline", "middle");
        text.setAttributeNS(null, "text-anchor", "middle");
        text.setAttributeNS(null, "x", xCenter);
        text.setAttributeNS(null, "y", yCenter);
        text.setAttributeNS(null, "style", "font-size: 15px;");

        const roomId = child.getAttribute("roomid");
        text.appendChild(document.createTextNode(roomIdToName(json, roomId)));

        roomLabelContainer.appendChild(text);

        // Adjust font size dynamically
        const textRect = text.getBBox();
        const fontGrowthRate = 5;

        let fontSize = fontGrowthRate * box.width / textRect.width;
        const minFontSize = 10;
        const maxFontSize = 20;

        if (fontSize < minFontSize) fontSize = minFontSize;
        if (fontSize > maxFontSize) fontSize = maxFontSize;

        text.setAttributeNS(null, "style", `font-size: ${fontSize}px;`);
    }

    assignOnClickEvents(children);
    centerScroll();
}


window.addEventListener("load", () => {
    centerScrollInitial();
});

let isLoaded = false;

//initially window.innerWidth will be 0 causing the scroll to not be correctly centered
function centerScrollInitial() {

    if (isLoaded) {
        return;
    }

    if (window.innerWidth === 0 || window.innerHeight === 0) {
        setTimeout(centerScrollInitial, 10);
        return;
    }
    isLoaded = true;
    centerScroll();
}

function centerScroll() {
    const svg = document.querySelector('svg');
    if (svg == null) {
        return;
    }
    const bbox = svg.getBoundingClientRect();

    const margin = parseFloat(window.getComputedStyle(svg).marginLeft);

    const centerX = margin + bbox.width / 2;
    const centerY = margin + bbox.height / 2;

    window.scrollTo({
        left: centerX - window.innerWidth / 2,
        top: centerY - window.innerHeight / 2
    })
}

function bringDoorsToFront() {
    const svg = document.querySelector('svg');
    if (svg == null) {
        return;
    }

    const drzwi = svg.querySelector('#drzwi');
    if (drzwi == null) {
        return;
    }

    drzwi.parentNode.appendChild(drzwi);
}

function assignOnClickEvents(rooms) {
    for (let i = 0; i < rooms.length; i++) {
        rooms[i].addEventListener("click", async (event) => {
            const clickedElement = event.currentTarget;

            for (let j = 0; j < rooms.length; j++) {
                rooms[j].classList.remove("activeRoom");
            }

            clickedElement.classList.add("activeRoom");

            const roomId = clickedElement.getAttribute("roomid");

            await window.HybridWebView.InvokeDotNet("ReceiveRoomClicked", [roomId]);
        });
    }
}

function ReceiveFloorSvg(svg) {
    const body = document.getElementsByTagName("body")[0];
    body.innerHTML = svg;
    centerScroll();
    bringDoorsToFront();
}

function ReceiveCampusSvg(svg) {
    const body = document.body;
    body.innerHTML = svg;

    const campusBuildings = document.getElementById("pomieszczenia").children;

    for (let i = 0; i < campusBuildings.length; i++) {
        campusBuildings[i].addEventListener("click", async (event) => {
            const clickedElement = event.currentTarget;
            let roomId = clickedElement.getAttribute("id");
            roomId = roomId.replace("warta_", "");

            await window.HybridWebView.InvokeDotNet("ReceiveCampusBuildingClicked", [roomId]);
        });
    }

    //setTimeout(centerScroll, 100);
}

function roomIdToName(parsedJson, id) {
    const numberOfNamesToShow = 1;

    const records = parsedJson.filter(record => record.roomId == id);

    if (records.length === 0) {
        console.log("returning emtpy");
        return "";
    }

    const names = records
        .sort((a, b) => b.nameWeight - a.nameWeight)
        .map(record => record.name)
        .slice(0, numberOfNamesToShow);

    console.log("returning = " + names.join(","));
    return names.join(", ");
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