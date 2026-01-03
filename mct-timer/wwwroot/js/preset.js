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
            plus10: root.querySelector("#plus10"),
            datevalue: root.querySelector("#datevalue"),
            brtype: root.querySelector("#brtype"),
            deftz: root.querySelector("#def-tz"),
            btypes: root.querySelectorAll('.type-icons'),
            defbtype: root.querySelector('#def-type-icons') 
        };


        this.el.defbtype.style.border = '3px solid black'; //default selection
        this.el.brtype.value = 'wait';//default selection

        this.el.btypes.forEach(btn => {
            btn.addEventListener("click",
                function () {
                    btn.parentElement.querySelectorAll('.type-icons').forEach(otherbtn => { otherbtn.style.border = 'none'; });
                    btn.style.border = '3px solid black';
                    document.querySelector("#brtype").value = btn.getAttribute('tp');                });
        });

        this.el.deftz.addEventListener("click", () => {
            const newTz = moment.tz.guess();
            $(this.el.timezone).val(newTz).trigger('change.select2');
            this.updateTimerForTimezoneChange(newTz);
        });

        // Initialize Select2 for searchable timezone dropdown
        $(this.el.timezone).select2({
            width: '100%',
            allowClear: false,
            placeholder: 'Search timezone...',
            matcher: function (params, data) {
                // If there are no search terms, return all of the data
                if ($.trim(params.term) === '') {
                    return data;
                }
                
                // Convert search term: replace spaces with underscores for matching
                var searchTerm = params.term.toUpperCase();
                var searchTermWithUnderscore = searchTerm.replace(/\s+/g, '_');
                
                // Do a recursive check for options with children
                if (data.children && data.children.length > 0) {
                    var filtered = $.map(data.children, function (child) {
                        var childText = child.text.toUpperCase();
                        // Match against both original search term and underscore version
                        if (childText.indexOf(searchTerm) >= 0 || childText.indexOf(searchTermWithUnderscore) >= 0) {
                            return child;
                        }
                    });
                    // If we have any filtered children, return the parent
                    if (filtered.length) {
                        var modifiedData = $.extend({}, data, true);
                        modifiedData.children = filtered;
                        return modifiedData;
                    }
                    // Otherwise return null
                    return null;
                }
                
                // If it doesn't have children, filter and return the user data and clone of the placeholder
                var dataText = data.text.toUpperCase();
                // Match against both original search term and underscore version
                if (dataText.indexOf(searchTerm) >= 0 || dataText.indexOf(searchTermWithUnderscore) >= 0) {
                    var modifiedData = $.extend({}, data, true);
                    return modifiedData;
                }                // Return null if the term does not match
                return null;
            }        });        // Handle Select2 change event to recalculate timer when timezone changes
        $(this.el.timezone).on('select2:select', (e) => {
            this.updateTimerForTimezoneChange(this.el.timezone.value);
        });

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
        this.el.datevalue.value = defaultDuration.format('YYYY-MM-DD');
          

        var timezonesames = [{ text: moment.tz.guess(), value: moment.tz.guess() }];

        try
        {

        timezonesames  = Object.values(moment.tz._zones)
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
            }            this.el.timezone.add(option);
        }

        // Note: timezone change is now handled by Select2:select and def-tz button events via updateTimerForTimezoneChange()

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

        this.el.plus10.addEventListener("click", () => {
            this.addMinutes(10);
        });

        this.el.tophour.addEventListener("click", () => {
            const currrentDuration = this.getTimerDuration().tz(this.tz, true);

            try {
                const newDuration = currrentDuration.add(1, 'h');
                newDuration.minute(0);
                newDuration.second(0);
                
                this.setTimerDuration(newDuration)
            } catch (e) {

                appInsights.TrackException(e);
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

            this.startTimer(this.tz, minutes, this.el.brtype.value);
        });
    }

    startTimer(timezone, duration, timerType) {

        appInsights.trackMetric({ name: "CustomTimer", tz: timezone, lenght: duration, tp: timerType  });

        const timezoneUrlEncoded = encodeURIComponent(timezone.replaceAll('/','@'));

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
        return moment(this.el.datevalue.value + ' ' + this.el.input.value + ' ' + this.el.ampm.value, 'YYYY-MM-DD hh:mm A');
    }
    
    setTimerDuration(duration) {
        this.el.input.value = duration.format('hh:mm');
        this.el.ampm.value = duration.format('A');
        this.el.datevalue.value = duration.format('YYYY-MM-DD');

        this.durationFieldUpdated();  // Force validation to re-run
    }


    addMinutes(quantity) {
        try {
            const currentDuration = this.getTimerDuration().tz(this.tz, true);
            const newDuration = new moment(currentDuration).add(quantity, 'm');
            newDuration.second(0);

            this.setTimerDuration(newDuration);
        } catch (e) {
            appInsights.TrackException(e);
            return;
        }
    }    setCookieByName(name, value, days) {
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

    updateTimerForTimezoneChange(newTz) {
        var oldTz = this.tz;
        this.tz = newTz;
        this.setCookieByName("tz", newTz, 30);

        const currrentDuration = this.getTimerDuration().tz(oldTz, true);
        const defaultDuration = currrentDuration.tz(this.tz);
        this.el.input.value = defaultDuration.format('hh:mm');
        this.el.ampm.value = defaultDuration.format('A');
        this.el.datevalue.value = defaultDuration.format('YYYY-MM-DD');
    }

}

