/**
 * Sets marquee animation for status tags on Cards on App Details page.
 * */
export function setStatusTagMarqueeAnimations() {
    // marquee animation code from: https://jsfiddle.net/mga8bh73/129/
    const tansitionTimePerPixel = 0.01;
    const marquees = document.querySelectorAll(".marquee");

    marquees.forEach((marquee) => {
        marquee.addEventListener('mouseenter', () => {
            let textWidth = marquee.lastChild.clientWidth;
            let boxWidth = parseFloat(getComputedStyle(marquee).width);
            let translateVal = Math.min(boxWidth - textWidth, 0);
            let translateTime = - tansitionTimePerPixel * translateVal + "s";
            marquee.lastChild.style.transitionDuration = translateTime;
            marquee.lastChild.style.transform = "translateX(" + translateVal + "px)";
        })
        marquee.addEventListener('mouseleave', () => {
            marquee.lastChild.style.transitionDuration = "0.3s";
            marquee.lastChild.style.transform = "translateX(0)";
        })
    });
}

export function toggleDropdown(className) {
    var dropdowns =
        document.getElementsByClassName("ant-dropdown");
    var open = false;

    for (let i = 0; i < dropdowns.length; i++) {
        if (dropdowns[i].classList.contains(className)) {

            if (dropdowns[i].style.display != 'none') {
                dropdowns[i].style.display = 'none';
                open = false;
            } else {
                dropdowns[i].style.display = 'inline-flex';
                open = true;
            }
        } else {
            dropdowns[i].style.display = 'none';
        }
    }
    return open;
}

export function closeAllDropdowns() {
    var dropdowns =
        document.getElementsByClassName("ant-dropdown");
    for (let i = 0; i < dropdowns.length; i++) {
        // Runs 5 times, with values of step 0 through 4.
        dropdowns[i].style.display = 'none';
    }
}