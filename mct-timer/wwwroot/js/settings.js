async function AJAXSubmit(oFormElement) {
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
                 oFormElement.elements.namedItem("result").value = "File upload successfully";

                 //hide upload show the create.
                 updateIcons(0);
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