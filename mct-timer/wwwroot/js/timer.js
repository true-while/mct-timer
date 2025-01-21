class Timer {
    constructor(timerroot, resumeroot) {              

        this.el = {
            timepart1: timerroot.querySelector("#timer__part1"),
            timepart2: timerroot.querySelector("#timer__part2"),
            timediv: timerroot.querySelector("#timer__devider"),
            home: timerroot.querySelector(".timer__btn--home"),
            reset: timerroot.querySelector(".timer__btn--reset"), 
            music: timerroot.querySelector(".timer__btn--music"), 
            whenpart1: resumeroot.querySelector("#when__part1"),
            whenpart2: resumeroot.querySelector("#when__part2"),
            whendiv: resumeroot.querySelector("#when__devider"),
            zone: resumeroot.querySelector("#zone"),
            zoneFull: resumeroot.querySelector("#zone-full"),
            am: resumeroot.querySelector("#am"), 
            timeisup: timerroot.querySelector("#time-is-up"),
            intro: timerroot.querySelector("#intro-music")
        };

        this.ampm = true;
        this.originalEnd;             //original end time
        this.originalSeconds = 0;    //original amount of seconds
        this.interval = null;       //timer for 1 second
        this.remainingSeconds = 0; // calculated value
        this.timezoneName;  //full time zone, should be pass outside 
        this.timezoneAbr;  //short timezone name, should be pass outside

        //stop and return to preset
        this.el.home.addEventListener("click", () => {
            this.stop();            
            location.href = '/';
        });
     

        //restart interval: add to the current time the timer length
        this.el.reset.addEventListener("click", () => {
            this.stop();   

            this.originalEnd = moment.tz(this.timezoneName).add(this.originalSeconds, 'seconds');

            this.updateInterfaceTime();

            this.start();
        });

        //play music
        this.el.music.addEventListener("click", () => {
            if (this.el.intro.currentTime === 0) {
                this.el.intro.play();
            } else {
                this.el.intro.pause();
                this.el.intro.currentTime = 0
            }
                

        });
    }

    //update numbers and colors.
    updateInterfaceTime() {



        this.remainingSeconds = moment.duration(this.originalEnd.diff(moment().tz(this.timezoneName))).asSeconds() 


        const hours = Math.floor(this.remainingSeconds / (60*60));
        const minutes = Math.floor((this.remainingSeconds / 60) - (hours * 60));
        const seconds = Math.floor(this.remainingSeconds - hours * (60*60) - minutes * 60);


        if (this.remainingSeconds <= 0) {
            this.el.timepart1.textContent = "00";
            this.el.timepart2.textContent = "00";
        } else if (hours === 0) {
            this.el.timepart1.textContent = minutes.toString().padStart(2, "0");
            this.el.timepart2.textContent = seconds.toString().padStart(2, "0");
        } else {
            this.el.timepart1.textContent = hours.toString().padStart(2, "0");
            this.el.timepart2.textContent = minutes.toString().padStart(2, "0");
        }
        
        if (this.remainingSeconds < 0) {
            this.el.timepart1.style.color = this.el.timepart2.style.color = this.el.timediv.style.color = "red";
        } else if (this.remainingSeconds < 30) {
            this.el.timeisup.play();
        } else if (this.remainingSeconds < 60) {           
            this.el.timepart1.style.color = this.el.timepart2.style.color = this.el.timediv.style.color = "red";
        } else if (this.remainingSeconds < 180) {
            this.el.timepart1.style.color = this.el.timepart2.style.color = this.el.timediv.style.color = "darkorange";
        } else {
            this.el.timepart1.style.color = this.el.timepart2.style.color = this.el.timediv.style.color = "black";
        }

        if (this.ampm) { this.el.whenpart1.textContent = this.originalEnd.format('h'); } else { this.el.whenpart1.textContent = this.originalEnd.format('HH'); }
        this.el.whenpart2.textContent = this.originalEnd.format('mm');
        this.el.whendiv.textContent = ":"
        if (this.ampm) { this.el.am.textContent = this.originalEnd.format('A'); }

        this.el.zone.textContent = this.timezoneAbr;
        this.el.zoneFull.textContent = this.timezoneName;
    }


    //start new 1sec interval
    start() {
        if (this.remainingSeconds === 0) return;

        this.interval = setInterval(() => {
            this.remainingSeconds--;
            this.updateInterfaceTime();

            if (this.remainingSeconds === 0) {
                this.stop();
            }
        }, 1000);

    }

    //stop for restart
    stop() {
        clearInterval(this.interval);

        this.interval = null;
    }
}




