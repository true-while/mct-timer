class Timer {
    constructor(timerroot, resumeroot) {              

        this.el = {
            timepart1: timerroot.querySelector("#timer__part1"),
            timepart2: timerroot.querySelector("#timer__part2"),
            timediv: timerroot.querySelector("#timer__devider"),
            control: timerroot.querySelector(".timer__btn--control"),
            reset: timerroot.querySelector(".timer__btn--reset"),         
            whenpart1: resumeroot.querySelector("#when__part1"),
            whenpart2: resumeroot.querySelector("#when__part2"),
            whendiv: resumeroot.querySelector("#when__devider"),
            zone: resumeroot.querySelector("#zone"),
            am: resumeroot.querySelector("#am") 
        };

        this.interval = null;
        this.remainingSeconds = 0;

        this.el.control.addEventListener("click", () => {
            if (this.interval === null) {
                this.start();
            } else {
                this.stop();
            }
        });

        this.el.reset.addEventListener("click", () => {
            const inputMinutes = parseInt(prompt("Enter number of minutes:"));

            this.stop();   

            this.remainingSeconds = inputMinutes * 60;

            this.updateInterfaceTime();

            this.start();
         
        });
    }

    updateInterfaceTime() {
        const hours = Math.floor(this.remainingSeconds / (60*60));
        const minutes = Math.floor((this.remainingSeconds / 60) - (hours * 60));
        const seconds = this.remainingSeconds - hours * (60*60) - minutes * 60;
        

        if (hours == 0) {
            this.el.timepart1.textContent = minutes.toString().padStart(2, "0");
            this.el.timepart2.textContent = seconds.toString().padStart(2, "0");
        } else {
            this.el.timepart1.textContent = hours.toString().padStart(2, "0");
            this.el.timepart2.textContent = minutes.toString().padStart(2, "0");
        }
        
        if (this.remainingSeconds < 60) {
            this.el.timepart1.style.color = this.el.timepart2.style.color = this.el.timediv.style.color = "red";
        } else if (this.remainingSeconds < 180) {
            this.el.timepart1.style.color = this.el.timepart2.style.color = this.el.timediv.style.color = "darkorange";
        } else {
            this.el.timepart1.style.color = this.el.timepart2.style.color = this.el.timediv.style.color = "black";
        }

        var enddate = moment().add(this.remainingSeconds, 's');
        this.el.whenpart1.textContent = enddate.format("hh");
        this.el.whenpart2.textContent = enddate.format("mm");
        this.el.whendiv.textContent = ":"
        this.el.am.textContent = enddate.format('A');
        this.el.zone.textContent = "EST"; //tz('America/Los_Angeles')
    }

    updateInterfaceControls() {
        if (this.interval === null) {
            this.el.control.innerHTML = `<span class="material-icons">play_arrow</span>`;
            this.el.control.classList.add("timer__btn--start");
            this.el.control.classList.remove("timer__btn--stop");
        } else {
            this.el.control.innerHTML = `<span class="material-icons">pause</span>`;
            this.el.control.classList.add("timer__btn--stop");
            this.el.control.classList.remove("timer__btn--start");
        }
    }

    start() {
        if (this.remainingSeconds === 0) return;

        this.interval = setInterval(() => {
            this.remainingSeconds--;
            this.updateInterfaceTime();

            if (this.remainingSeconds === 0) {
                this.stop();
            }
        }, 1000);

        this.updateInterfaceControls();
    }

    stop() {
        clearInterval(this.interval);

        this.interval = null;

        this.updateInterfaceControls();
    }
}

new Timer(
    document.querySelector("#timer"),
    document.querySelector("#resume")
);



