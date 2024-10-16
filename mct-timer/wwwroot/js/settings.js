class Settings {
    constructor(root, defUserTZ) {

        this.el = {
            timezone: root.querySelector("#DefTZ"),
            ampm: root.querySelector("#Ampm"),
            lang: root.querySelector("#Language"),
            form: root.querySelector('#form-settings'),
            cbg: root.querySelector('#cBg'),
            dbg: root.querySelector('#dBg'),
        };


        this.tz = moment.tz.guess();
        if (!this.tz) {
            this.tz = "America/New_York";
        }

        if (defUserTZ) this.tz = defUserTZ;

        var timezonesames = [{ text: moment.tz.guess(), value: moment.tz.guess() }];

        try {

            timezonesames = Object.values(moment.tz._zones)
                .filter(function (k) {
                    var name = k.name;
                    if (!name) name = k;
                    return name.indexOf('/') >= 0 && name.indexOf('Etc') != 0;
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
            appInsights.TrackException(e);
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
            this.el.form.submit();
        });

        this.el.ampm.addEventListener("change", () => {
            this.el.form.submit();
        });

        this.el.lang.addEventListener("change", () => {
            this.el.form.submit();
        });

        this.el.cbg.addEventListener("click", () => {
            location.href = '/cbg';
        });
        this.el.dbg.addEventListener("click", () => {
            location.href = '/dbg';
        });
    }
}



//function imageErrorHandler(evt) {
//    var $e = $(this);
//    var src = $e.attr("src");
//    console.log("Image URL '" + src + "' is invalid.");
//};



class CustomBg {
    constructor(root) {

        this.el = {
            progress: root.querySelector("#progress"),
            bar: root.querySelector('#progress-bar'),
            icons: document.querySelectorAll(".icons"),
            file: document.querySelector('#file'),
            details: document.querySelector('#details'),
            createBtn: document.querySelector('#create'),
            form: document.querySelector('#uploadForm'),
            btnUpload: document.querySelector('#btnUpload'),
            uplResult: document.querySelector('#result'),
        };

        this.el.createBtn.addEventListener('click', () => {
            this.refreshIcons(0);
        })
    }
    

    StartProgressBar() {

        this.el.progress.style.visibility = 'visible';
        
        //bar.removeClass("progress-bar").addClass("progress-bar");
        this.el.bar.animation = 'none';
        this.el.bar.offsetHeight;
        this.el.bar.style.animation = null;
    }
    StopProgressBar() {

        this.el.progress.style.visibility = 'hidden';
    }

    OnBGCreate() {

        this.el.file.style.display = ''
        this.el.details.style.display = 'none';

        refreshIcons(0);
    }


    OnFileUploaded() {
        //clean error messages
        this.el.uplResult.value = "";

        //hide upload controls
        this.el.file.style.display = 'none'

        //show details control
        this.el.details.style.display = '';
    }

    ShowValidaitonError(eror) {
        //file validation errors:
        this.el.uplResult.value = eror;
    }
    


    refreshIcons(counter) {
        counter++;
        var self = this;

        this.el.icons.forEach(img => {

            $.ajax({
                url: img.src,
                cache: false,
                success: function (html) {
                    console.log("working");
                    if (img.interval) {
                        clearInterval(img.interval);
                        img.interval = null;
                    }

                },
                error: function (html) {
                    img.interval = setInterval(() => {
                        if (!img.src.includes('?')) {
                            img.src = `${img.src}?${Date.now()}`;
                        } else {
                            img.src =
                                img.src.slice(0, img.src.indexOf('?') + 1) +
                                Date.now();
                        }
                    }, 2000);
                    if (counter && counter < 10) self.refreshIcons(counter);
                }
            });



            //$.ajax({
            //    url: img.src,
            //    cache: false,
            //}).done(function (result) {
            //    console.log("working");
            //    if (img.interval)
            //          img.interval.stop();
            //}).fail(function (ex) {
            //    //img.interval = setInterval(() => {
            //    //    if (!img.src.includes('?')) {
            //    //        img.src = `${img.src}?${Date.now()}`;
            //    //    } else {
            //    //        img.src =
            //    //            img.src.slice(0, img.src.indexOf('?') + 1) +
            //    //            Date.now();
            //    //    }
            //    //}, 1000);
            //});




            //img.addEventListener('load', ()=> {
            //    if (img.interval)
            //        img.interval.stop();
            //}, false);

            //img.addEventListener('error', ()=> {
            //    setInterval(() => {
            //        var newUrl = new Url(i.src);
            //        newUrl.search = "?t=" + new Date().getTime();
            //        img.src = newUrl.href;
            //    }, 1000);
            //}, false);

            //img.src = img.src;


        });
    }
}
