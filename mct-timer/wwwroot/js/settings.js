﻿function OnBGCreate() { 
    
    var file = document.getElementById('file');
    file.style.display = '' 
    var details = document.getElementById('details');
    details.style.display = 'none'; 
    
    updateIcons(0);
}
function OnFileUploaded() {

    var file = document.getElementById('file');
    file.style.display = 'none'
    var details = document.getElementById('details');
    details.style.display = '';
}


async function AJAXSubmit(oFormElement) {
    var progerss = document.getElementById('progress');
    progerss.style.visibility = 'visible'; 
    var bar = document.getElementById('progress-bar');
    //bar.removeClass("progress-bar").addClass("progress-bar");
    bar.style.animation = 'none';
    bar.offsetHeight;
    bar.style.animation = null;        

    const formData = new FormData(oFormElement);
    var status;
    const response = await fetch(oFormElement.action, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': getCookie('RequestVerificationToken')
        },
        body: formData
    }).then(
        response => {            
            status = response.status;
            return response.json();
     }).then(errors => {  

         switch (status) {
                case 201:
                case 200:
                 oFormElement.elements.namedItem("result").value = "";
                 OnFileUploaded()
                 break;
             case 400:
                 //file validation errors:
                 oFormElement.elements.namedItem("result").value = errors.File[0];
                 break;

             default:
                 oFormElement.elements.namedItem("result").value = "There is a problem with your file:" + errors.File[0];
                    break;
            }
        });


    progress.style.visibility = 'hidden';
}

function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length == 2) return parts.pop().split(";").shift();
}

//function imageErrorHandler(evt) {
//    var $e = $(this);
//    var src = $e.attr("src");
//    console.log("Image URL '" + src + "' is invalid.");
//};

function updateIcons(counter) {
    counter++;
    var icons = document.querySelectorAll(".icons")


    icons.forEach(img => {

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
                if (counter && counter < 10) updateIcons(counter);
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

class Settings {
    constructor(root, defUserTZ) {

        this.el = {
            timezone: root.querySelector("#DefTZ"),
            ampm: root.querySelector("#Ampm"),
            lang: root.querySelector("#Language"),
            form: root.querySelector('#form-settings')
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

}
}