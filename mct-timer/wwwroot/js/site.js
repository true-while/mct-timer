
class BGSelector {
    constructor(root, btype) {

        switch (btype) {
            case "preset":
                root.style.backgroundImage = "url(/bg-lib/index.jpg)";
                break;
            case "settings":
                root.style.backgroundImage = "url(/bg-lib/index.jpg)";
                break;
            case "info":
                root.style.backgroundImage = "url(/bg-lib/info.jpg)";
                break;
            case "coffee":
                root.style.backgroundImage = "url(/bg-lib/coffee.jpg)";
                break;
            case "lunch":
                root.style.backgroundImage = "url(/bg-lib/lunch.jpg)";
                break;
            case "labs":
                root.style.backgroundImage = "url(/bg-lib/labs.jpg)";
                break;
            case "wait":
                root.style.backgroundImage = "url(/bg-lib/wait.jpg)";
                break;
            case "inprogress":
                root.style.backgroundImage = "url(/bg-lib/inprogress.jpg)";
                break;
            case "account":
                root.style.backgroundImage = "url(/bg-lib/account.jpg)";
                break;
            default:
                root.style.backgroundImage = "url(/bg-lib/break.jpg)";
                break; 

        }

    }
}




