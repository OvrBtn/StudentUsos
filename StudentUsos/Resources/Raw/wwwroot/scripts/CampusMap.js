
body = document.getElementsByTagName("body")[0];

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

        let localCenter = calculateCenter(children[index]);
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

        fontSize = 15 * box.width * 0.3 / textRect1.width
        minFontSize = 14
        maxFontSize = 200000
        if (fontSize < minFontSize) fontSize = minFontSize
        if (fontSize > maxFontSize) fontSize = maxFontSize
        text.setAttributeNS(null, "style", "font-size: " + fontSize + "px;")

        centerScroll();
    }
}

function centerScroll() {
    const svg = document.querySelector('svg');
    const bbox = svg.getBBox();

    const centerX = bbox.x + bbox.width / 2;
    const centerY = bbox.y + bbox.height / 2;

    window.scrollTo({
        left: centerX - window.innerWidth / 2,
        top: centerY - window.innerHeight / 2,
        behavior: 'smooth'
    })
}

function ReceiveFloorSvg(svg) {
    body.innerHTML = svg;
}

function roomIdToName(parsedJson, id) {
    names = [];
    for (let i = 0; i < parsedJson.length; i++) {
        if (parsedJson[i].roomId == id) {
            names.push(parsedJson[i].name);
        }
    }
    if (names.length > 0) {
        return names.join(",");
    }
    return "";
}

function calculateCenter(svgPath) {
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