class Preset {
    constructor(root) {
     
        this.el = {
            imp: root.querySelector("#custom-imput"),
            datevalue: root.querySelector("#datevalue"),
            ampm: root.querySelector("#ampm"),
            go: root.querySelector("#go"),
            tophour: root.querySelector("#tophour"),
            plus1: root.querySelector("#plus1"),
            minus1: root.querySelector("#minus1"),
            plus5: root.querySelector("#plus5"),
            
        };

        this.el.datevalue.value = moment();
        this.el.imp.value = moment().format('hh:mm');
        this.el.ampm.value = moment().format('A');

        this.el.imp.addEventListener("change", () => {           
            try {
                this.el.datevalue.value = moment(this.el.imp.value + ' ' + this.el.ampm.value, 'hh:mm A');
                this.el.imp.style.color = "black";
            } catch (e) {
                alert('please provide valid time in format {hours:minutes}');
                return;
            }
        });

        this.el.ampm.addEventListener("change", () => {
                this.addMinutes(12*60);  //add 12 hours
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


            var cur = this.getCurrentValue();
           

            try {               
                var newtime = new moment(cur).add(1, 'h');
                newtime.minute(0);
                newtime.second(0);
                this.el.imp.value = newtime.format('hh:00');
                this.el.ampm.value = newtime.format('A');
                this.el.datevalue.value = newtime;
                this.el.imp.style.color = "black";

            } catch (e) {
                return;
            }

        });

        this.el.go.addEventListener("click", () => {

            var end = this.getCurrentValue();

            var mins = Math.floor(moment.duration(end.diff(moment())).add(1,'m').asMinutes());
            if (mins > 0) {
                this.el.imp.style.color = "black";
                location.href = `./timer/${mins}/wait`;
            } else {
                this.el.imp.style.color = "red";
            }
        });

    }

    getCurrentValue() {
        
            return new moment(this.el.datevalue.value);


    }

    addMinutes(quantity) {
        try {

            var cur =  this.getCurrentValue();
            var newtime = new moment(cur).add(quantity, 'm');
            newtime.second(0);

            if (newtime > moment()) {
                this.el.imp.value = newtime.format('hh:mm');
                this.el.ampm.value = newtime.format('A');
                this.el.datevalue.value = newtime;
                this.el.imp.style.color = "black";
            }

        } catch (e) {
            return;
        }

    }
}

