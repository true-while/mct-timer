class Timer {
    constructor(timerroot, resumeroot) {              

        this.el = {
            timepart1: timerroot.querySelector("#timer__part1"),
            timepart2: timerroot.querySelector("#timer__part2"),
            timediv: timerroot.querySelector("#timer__devider"),
            home: timerroot.querySelector(".timer__btn--home"),
            reset: timerroot.querySelector(".timer__btn--reset"),         
            whenpart1: resumeroot.querySelector("#when__part1"),
            whenpart2: resumeroot.querySelector("#when__part2"),
            whendiv: resumeroot.querySelector("#when__devider"),
            zone: resumeroot.querySelector("#zone"),
            am: resumeroot.querySelector("#am") 
        };

        this.originalSeconds = 0;
        this.interval = null;
        this.remainingSeconds = 0;

        this.el.home.addEventListener("click", () => {
            this.stop();
            
            location.href = '/';
        });


        this.el.reset.addEventListener("click", () => {
            this.stop();   

            this.remainingSeconds = this.originalSeconds

            this.updateInterfaceTime();

            this.start();
        });
    }

    updateInterfaceTime() {
        const hours = Math.floor(this.remainingSeconds / (60*60));
        const minutes = Math.floor((this.remainingSeconds / 60) - (hours * 60));
        const seconds = this.remainingSeconds - hours * (60*60) - minutes * 60;

        if (this.remainingSeconds <= 0) {
            this.el.timepart1.textContent = "--";
            this.el.timepart2.textContent = "--";
        } else if (hours === 0) {
            this.el.timepart1.textContent = minutes.toString().padStart(2, "0");
            this.el.timepart2.textContent = seconds.toString().padStart(2, "0");
        } else {
            this.el.timepart1.textContent = hours.toString().padStart(2, "0");
            this.el.timepart2.textContent = minutes.toString().padStart(2, "0");
        }
        
        if (this.remainingSeconds < 0) {
            this.el.timepart1.style.color = this.el.timepart2.style.color = this.el.timediv.style.color = "red";
        } else if (this.remainingSeconds < 60) {
            this.el.timepart1.style.color = this.el.timepart2.style.color = this.el.timediv.style.color = "red";
        } else if (this.remainingSeconds < 180) {
            this.el.timepart1.style.color = this.el.timepart2.style.color = this.el.timediv.style.color = "darkorange";
        } else {
            this.el.timepart1.style.color = this.el.timepart2.style.color = this.el.timediv.style.color = "black";
        }

        const endDate = moment().tz(timezoneName).add(this.remainingSeconds, 's');
        this.el.whenpart1.textContent = endDate.format("hh");
        this.el.whenpart2.textContent = endDate.format("mm");
        this.el.whendiv.textContent = ":"
        this.el.am.textContent = endDate.format('A');

        const split = timezoneName.split("/");
        const timeZoneShortName = split[split.length-1].replace("_", " ");
        
        this.el.zone.textContent = timeZoneShortName;
    }

    updateInterfaceControls() {
        //if (this.interval === null) {
        //    this.el.control.innerHTML = `<span class="material-icons">play_arrow</span>`;
        //    this.el.control.classList.add("timer__btn--start");
        //    this.el.control.classList.remove("timer__btn--stop");
        //} else {
        //    this.el.control.innerHTML = `<span class="material-icons">pause</span>`;
        //    this.el.control.classList.add("timer__btn--stop");
        //    this.el.control.classList.remove("timer__btn--start");
        //}
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




