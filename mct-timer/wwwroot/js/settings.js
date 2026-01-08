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


class BgList {
    constructor(root) {

        this.el = {
            bgs: root.querySelectorAll(".bg-item")
        };


        this.el.bgs.forEach(bg => {

            var img = bg.querySelector(".bg-review");
            var src = img.getAttribute("org-src").replace('/s/','/m/');
            var title = "Background type: " + img.getAttribute("tp") ;
            img.addEventListener('click', () => {
                $("<div title='" + title + "'><img src='" + src + "'></div>")
                    .dialog({
                        minWidth: 550,
                        modal: true
                    }).position({
                        my: "center",
                        at: "center",
                        });
            });

            var info = bg.querySelector(".icon-info");
            info.addEventListener('click', () => {
                $("<div title='Uploaded background info'><table cellpadding=10px><tr><td><img style='width:200px' src='" + src + "'></td><td><div>Type: " + img.getAttribute("tp") + "</div><div>Info: " + img.getAttribute("info") + "</div><div>Author: " + img.getAttribute("author") + "</div></td></tr></table>")
                    .dialog({
                        minWidth: 400,
                        modal: true
                    }).position({
                        my: "center",
                        at: "center",
                    });
            });
        });

    }

}



class CustomBg {
    constructor(root) {

        this.el = {
            progress: root.querySelector("#progress"),
            
            icons: document.querySelectorAll(".bg-review"),
            backbtn: document.querySelectorAll(".back"),
            file: document.querySelector('#file'),
            createBtn: document.querySelector('#create'),
            form: document.querySelector('#uploadForm'),
            btnUpload: document.querySelector('#btnUpload'),
            uplResult: document.querySelector('#result'),

            optChoise: document.querySelector('#optChoise'),
            toUpload: document.querySelector('#toUpload'),
            toGenerate: document.querySelector('#toGenerate'),


            optUpload: document.querySelector('#optUpload'),
            optGenerate: document.querySelector('#optGenerate'),
            optSubmit: document.querySelector('#optSubmit'),

            genInfo: document.querySelector('#genInfo'),
            uplInfo: document.querySelector('#uplInfo'),
         

        };        this.interval = new Map();

        this.el.backbtn.forEach(btn => {
            btn.addEventListener('click', () => {
                this.ShowChoise();
                this.HideGenerate();
                this.HideSubmit();
                this.HideUpload();
            })
        });

        if (this.el.createBtn) {
            this.el.createBtn.addEventListener('click', () => {
                this.refreshIcons(0);
            });
        }

        if (this.el.toUpload) {
            this.el.toUpload.addEventListener('click', () => {
                this.HideChoise();
                this.HideGenerate();
                this.HideSubmit();
                this.ShowUpload();
            });
        }        if (this.el.toGenerate) {
            this.el.toGenerate.addEventListener('click', () => {
                this.HideChoise();
                this.ShowGenerate();
                this.HideSubmit();
                this.HideUpload();
            });
        }

    }
    HideGenerate() {
        if (this.el.optGenerate) {
            this.el.optGenerate.style.display = 'none';
        }
        if (this.el.genInfo && this.el.uplInfo) {
            this.el.genInfo.value = this.el.uplInfo.value = ''; //remove entered text
        }
    }

    ShowGenerate() { 
        if (this.el.optGenerate) {
            this.el.optGenerate.style.display = ''; 
        }
    }

    HideSubmit() {
        if (this.el.optSubmit) {
            this.el.optSubmit.style.display = 'none';
        }
        if (this.el.genInfo && this.el.uplInfo) {
            this.el.genInfo.value = this.el.uplInfo.value = ''; //remove entered text
        }
    }

    ShowSubmit() { 
        if (this.el.optSubmit) {
            this.el.optSubmit.style.display = ''; 
        }
    }

    HideUpload() { 
        if (this.el.optUpload) {
            this.el.optUpload.style.display = 'none'; 
        }
    }

    ShowUpload() { 
        if (this.el.optUpload) {
            this.el.optUpload.style.display = ''; 
        }
    }

    HideChoise() { 
        if (this.el.optChoise) {
            this.el.optChoise.style.display = 'none'; 
        }
    }

    ShowChoise() { 
        if (this.el.optChoise) {
            this.el.optChoise.style.display = ''; 
        }
    }

    StartProgressBar() {
        if (this.el.progress) {
            this.el.progress.style.visibility = 'visible';
            this.el.progress.style.animation = 'none';
            this.el.progress.offsetHeight; /* trigger reflow */
            this.el.progress.style.animation = null; 
        }
    }

    StopProgressBar() {
        if (this.el.progress) {
            this.el.progress.style.visibility = 'hidden';
        }
    }


    OnFileUploaded() {
        //clean error messages
        this.el.uplResult.value = "";
        this.HideUpload();
        this.ShowSubmit();


    }

    ShowValidaitonError(eror) {
        //file validation errors:
        this.el.uplResult.value = eror;
    }

    OnBGCreate() {
        this.HideUpload();
        this.HideSubmit();
        this.HideGenerate();
        this.ShowChoise();

        this.el.optSubmit.style.display = 'none';

        refreshIcons(0);
    }      refreshTheIcon(root, theurl, img, counter) {
        // Clear any existing retry interval
        if (this.interval.get(theurl)) {
            clearInterval(this.interval.get(theurl));
            this.interval.set(theurl, null);
        }

        const self = this;

        $.ajax({
            url: theurl,
            cache: false,
            method: "HEAD",
            timeout: 5000, // Add 5 second timeout
            success: function (html) {
                img.src = theurl;
            },            error: function (html) {
                // Only retry on 404 errors (file not yet uploaded), not on other failures
                if (counter && counter < 10 && html.status == 404) {

                    self.interval.set(theurl, setTimeout(() => {

                        var orgLink = img.getAttribute("org-src");
                        // Add timestamp for cache busting without modifying org-src
                        var urlWithTs;
                        if (!orgLink.includes('?')) {
                            urlWithTs = `${orgLink}?ts=${Date.now()}`;
                        } else {
                            urlWithTs = orgLink.split('?')[0] + `?ts=${Date.now()}`;
                        }
                        
                        self.refreshTheIcon(self, urlWithTs, img, counter + 1);

                    }, 5000)); // 5 second delay between retries (~25 seconds total for 6 attempts)
                }

            }
        });
    }

    refreshIcons(counter) {
        // Process icons sequentially with delays to prevent resource exhaustion
        const icons = Array.from(this.el.icons);
        let index = 0;
        const self = this;

        const processNextIcon = () => {
            if (index < icons.length) {
                const img = icons[index];
                self.refreshTheIcon(self, img.getAttribute("org-src"), img, 1);
                index++;
                // Stagger requests by 300ms to reduce concurrent load
                setTimeout(processNextIcon, 300);
            }
        };

        processNextIcon();
    }
}
