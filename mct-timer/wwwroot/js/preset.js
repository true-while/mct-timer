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

        this.state = this.STATE_VALID;

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

        this.el.input.value = moment().format('hh:mm');
        this.el.ampm.value = moment().format('A');

        // Fill in the timezone
        let usertimeZone = moment.tz.guess();

        if (!usertimeZone) {
            usertimeZone = "America/New_York";
        }

        for (const tz of moment.tz.names()) {
            const option = document.createElement("option");
            option.text = tz;
            option.value = tz;

            if (tz === usertimeZone) {
                option.selected = true;
            }

            this.el.timezone.add(option);
        }

        this.el.input.addEventListener("change", () => {
            this.onDurationFieldChanged();
        });

        this.el.ampm.addEventListener("change", () => {
            this.addMinutes(12 * 60);  //add 12 hours
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
            const currrentDuration = this.getTimerDuration();

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

            const timezone = this.el.timezone.value;

            const duration = this.getTimerDuration().diff(moment(), 'minutes');
            const timezoneUrlEncoded = encodeURIComponent(timezone);

            location.href = `./timer/${duration}/${timezoneUrlEncoded}/wait`;
        });
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
        const duration = this.getTimerDurationField();
        
        const durationMoment = moment(duration, 'hh:mm');

        if (!durationMoment.isValid()) {
            this.setValidationState(this.STATE_INVALID);
            return;
        }

        const minutes = Math.floor(moment.duration(durationMoment.diff(moment())).add(1, 'm').asMinutes());

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
            const currentDuration = this.getTimerDuration();
            const newDuration = new moment(currentDuration).add(quantity, 'm');
            newDuration.second(0);

            this.setTimerDuration(newDuration);
        } catch (e) {
            return;
        }

    }
}

