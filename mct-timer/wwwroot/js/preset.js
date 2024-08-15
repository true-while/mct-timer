class Preset {
    constructor(root) {

       this.el = {
            imp: root.querySelector("#custom-imput"),
            ampm: root.querySelector("#ampm"),
            go: root.querySelector("#go"),
            tophour: root.querySelector("#tophour"),
            plus1: root.querySelector("#plus1"),
            minus1: root.querySelector("#minus1"),
            plus5: root.querySelector("#plus5"),
        };

        this.el.imp.value = moment().format('hh:mm');
        this.el.ampm.value = moment().format('A');

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

