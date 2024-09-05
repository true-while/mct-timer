class Preset {
    validationStates = [
        {
            valid: false,
            message: 'NULL',
        },
        {
            valid: true,
            message: '',
        },
        {
            valid: false,
            message: 'please provide valid time in format {hours:minutes}',
        },
        {
            valid: false,
            message: 'please provide a time in the future',
        }
    ];

    STATE_VALID = 1
    STATE_INVALID = 2
    STATE_PAST = 3

    constructor(root) {
        
        this.el = {
            input: root.querySelector("#custom-input"),
            ampm: root.querySelector("#ampm"),
            timezone: root.querySelector("#timezone"),
            go: root.querySelector("#go"),
            tophour: root.querySelector("#tophour"),
            plus1: root.querySelector("#plus1"),
            minus1: root.querySelector("#minus1"),
            plus5: root.querySelector("#plus5"),
        };

        this.state = this.STATE_VALID;
        this.tz = this.getCookieByName("tz");  //get cookies timezone.

        if (!this.tz) {
            this.tz = moment.tz.guess();
            if (!this.tz) {
                this.tz = "America/New_York";
            }
        }

        const defaultDuration = moment().tz(this.tz); //.add(1, 's');
        this.el.input.value = defaultDuration.format('hh:mm');
        this.el.ampm.value = defaultDuration.format('A');

          

        var timezonesames = [{ text: moment.tz.guess(), value: moment.tz.guess() }];

        try
        {

        timezonesames  = Object.values(moment.tz._zones)
            .filter(function (k) {
                    var name = k.name;
                    if (!name) name = k;
                    return name.indexOf('/') >= 0;
                })
            .map(function (k) {
                var name = k.name;
                if (!name) name = k;
                var tz = name.split('|')[0];
                var utc = moment.tz(tz).format('Z');
                return { offset: utc, text: utc + " | " + tz, value: tz, order: parseFloat(utc.replace(':', '.')) };
            })
                .sort(function (a, b) {
                    return a.order - b.order;
                });

        } catch (e) {
            timezonesames = [{ text: moment.tz.guess(), value: moment.tz.guess() }];
        }


        for (const tz of timezonesames) {
            const option = document.createElement("option");
            option.text = tz.text;
            option.value = tz.value;

            if (tz.value === this.tz) {
               option.selected = true;
            }

            this.el.timezone.add(option);
        }

        this.el.timezone.addEventListener("change", () => {
            var oldTz = this.tz;
            this.tz = this.el.timezone.value;
            this.setCookieByName("tz", this.el.timezone.value, 30);

            const currrentDuration = this.getTimerDuration().tz(oldTz,true);
            const defaultDuration = currrentDuration.tz(this.tz);
            this.el.input.value = defaultDuration.format('hh:mm');
            this.el.ampm.value = defaultDuration.format('A');
        });

        this.el.input.addEventListener("change", () => {
            this.onDurationFieldChanged();
        });

        this.el.plus1.addEventListener("click", () => {
            this.addMinutes(1);
        });

        this.el.minus1.addEventListener("click", () => {
            this.addMinutes(-1);
        });

        this.el.plus5.addEventListener("click", () => {
            this.addMinutes(5);
        });

        this.el.tophour.addEventListener("click", () => {
            const currrentDuration = this.getTimerDuration().tz(this.tz, true);

            try {
                const newDuration = currrentDuration.add(1, 'h');
                newDuration.minute(0);
                newDuration.second(0);
                
                this.setTimerDuration(newDuration)
            } catch (e) {
                return;
            }
        });

        this.el.go.addEventListener("click", () => {
            this.durationFieldUpdated();

            if (!this.getValidationState().valid) {
                alert(this.getValidationState().message);
                return;
            }

            const durationMoment = this.getTimerDuration().tz(this.tz, true); //moment(duration, 'hh:mm');

            const minutes = Math.ceil(
                moment.duration(durationMoment.diff(moment().tz(this.tz))).asSeconds() / 60
            );

            this.startTimer(this.tz, minutes, 'wait');
        });
    }

    startTimer(timezone, duration, timerType) {
        const timezoneUrlEncoded = encodeURIComponent(timezone.replace('/','.'));

        const timerUri = `./timer/${duration}/${timezoneUrlEncoded}/${timerType}`;

        location.href = timerUri;
    }
    
    startDefaultTimer(duration, timerType) {
        const timezone = this.el.timezone.value;
        
        this.startTimer(timezone, duration, timerType);
    }

    getValidationState() {
        return this.validationStates[this.state];
    }

    setValidationState(state) {
        this.state = state;

        if (this.getValidationState().valid) {
            this.el.input.style.color = "black";
        } else {
            this.el.input.style.color = "red";
        }
    }

    onDurationFieldChanged() {
        this.durationFieldUpdated()
    }

    durationFieldUpdated() {
        //const duration = this.getTimerDurationField();
        
        const durationMoment = this.getTimerDuration().tz(this.tz,true); //moment(duration, 'hh:mm');

        if (!durationMoment.isValid()) {
            this.setValidationState(this.STATE_INVALID);
            return;
        }

        const minutes = Math.ceil(
            moment.duration(durationMoment.diff(moment().tz(this.tz))).asSeconds() / 60
        );

        if (minutes <= 0) {
            this.setValidationState(this.STATE_PAST);
            return;
        }
        
        this.setValidationState(this.STATE_VALID);

        this.timerDuration = minutes;
    }

    getTimerDuration() {
        return moment(this.el.input.value + ' ' + this.el.ampm.value, 'hh:mm A');
    }
    
    setTimerDuration(duration) {
        this.el.input.value = duration.format('hh:mm');
        this.el.ampm.value = duration.format('A');

        this.durationFieldUpdated();  // Force validation to re-run
    }

    setTimerDurationField(duration) {
        this.el.input.value = duration;

        this.durationFieldUpdated();  // Force validation to re-run
    }
    
    getTimerDurationField() {
        return this.el.input.value;
    }

    addMinutes(quantity) {
        try {
            const currentDuration = this.getTimerDuration().tz(this.tz, true);
            const newDuration = new moment(currentDuration).add(quantity, 'm');
            newDuration.second(0);

            this.setTimerDuration(newDuration);
        } catch (e) {
            console.error(e)
            return;
        }
    }

    setCookieByName(name, value, days) {
        var exp = "";
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            exp = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + (value || "") + exp + "; path=/";
    }

    getCookieByName(name) {
        var name = name + "=";
        var ca = document.cookie.split(';');
        for (var j = 0; j < ca.length;j++) {
            var c = ca[j];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(name) == 0) return c.substring(name.length, c.length);
        }
        return null;
    }

}

