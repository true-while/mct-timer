class Preset {
    constructor(root) {

        this.el = {
            imp: root.querySelector("#custom-imput"),
            ampm: root.querySelector("#ampm"),
            timezone: root.querySelector("#timezone"),
            go: root.querySelector("#go"),
            tophour: root.querySelector("#tophour"),
            plus1: root.querySelector("#plus1"),
            minus1: root.querySelector("#minus1"),
            plus5: root.querySelector("#plus5"),
        };

        this.el.imp.value = moment().format('hh:mm');
        this.el.ampm.value = moment().format('A');

        // Fill in the timezone
        var usertimeZone = moment.tz.guess();

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

            try {

                const cur = new moment(this.el.imp.value + ' ' + ampm.value, 'hh:mm A');
                const tophour = new moment(cur).add(1, 'h');
                this.el.imp.value = tophour.format('hh:00');
                this.el.ampm.value = tophour.format('A');

            } catch (e) {
                return;
            }

        });

        this.el.go.addEventListener("click", () => {

            var end;

            try {

                end = new moment(this.el.imp.value + ' ' + ampm.value, 'hh:mm A');

            } catch (e) {
                alert('please provide valid time in format {hours:minutes}');
                return;
            }

            if (!end.isValid()) {
                alert('please provide valid time in format {hours:minutes}');
                return;
            }

            if (end.isBefore(moment())) {
                alert('please provide a time in the future');
                return;
            }
            
            const timezone = this.el.timezone.value;
            
            const timezoneUrlEncoded = encodeURIComponent(timezone);
            
            var mins = Math.floor(moment.duration(end.diff(moment())).add(1, 'm').asMinutes());
            location.href = `./timer/${mins}/${timezoneUrlEncoded}/wait`;
        });

    }

    addMinutes(quantity) {
        try {

            var cur = new moment(this.el.imp.value + ' ' + ampm.value, 'hh:mm A');
            var tophour = new moment(cur).add(quantity, 'm');
            this.el.imp.value = tophour.format('hh:mm');
            this.el.ampm.value = tophour.format('A');

        } catch (e) {
            return;
        }

    }
}

