
class BGSelector {
    constructor(root, btype) {
        var index = Math.floor(Math.random() * 5);  // from 0 to 4

        switch (btype) {
            case "preset":
                root.style.backgroundImage = "url(/bg-lib/index.jpg)";
                break;
            case "settings":
                root.style.backgroundImage = "url(/bg-lib/settings.jpg)";
                break;
            case "info":
                root.style.backgroundImage = "url(/bg-lib/info.jpg)";
                break;
            case "coffee":
            case "Coffee":
                root.style.backgroundImage = `url(/bg-lib/coffee${index}.jpg)`;
                break;
            case "lunch":
            case "Lunch":
                root.style.backgroundImage = "url(/bg-lib/lunch.jpg)";
                break;
            case "labs":
            case "Lab":
                root.style.backgroundImage = "url(/bg-lib/labs.jpg)";
                break;
            case "wait":
            case "Wait":
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




