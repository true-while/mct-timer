class Preset {
    constructor(root) {

       this.el = {
            imp: root.querySelector("#custom-imput"),
            ampm: root.querySelector("#ampm"),
            go: root.querySelector("#go"),
            tophour: root.querySelector("#tophour"),
            plus5: root.querySelector("#plus5")
        };

        this.el.imp.value = moment().format('hh:mm');
        this.el.ampm.value = moment().format('A');

        this.el.plus5.addEventListener("click", () => {

            try {

                var cur = new moment(this.el.imp.value + ' ' + ampm.value, 'hh:mm A');
                var tophour = new moment(cur).add(5, 'm');
                this.el.imp.value = tophour.format('hh:mm');
                this.el.ampm.value = tophour.format('A');

            } catch (e) {
                return;
            }

        });

        this.el.tophour.addEventListener("click", () => {

            try {

                var cur = new moment(this.el.imp.value + ' ' + ampm.value, 'hh:mm A');
                var tophour = new moment(cur).add(1, 'h');
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

            } catch (e)
            {
                alter('please provide valid time in format {hours:minutes}');
                return;
            }

            var mins = Math.floor(moment.duration(end.diff(moment())).add(1,'m').asMinutes());
            if (mins > 0) {
                location.href = `./timer/${mins}/wait`;
            } 
        });

    }
}

