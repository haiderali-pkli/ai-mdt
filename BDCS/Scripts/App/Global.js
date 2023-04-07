var HeartbeatTimer;
function loadPage(url, name, targetElement)
{
    showProgressBar();
    debugger
    targetElement = targetElement || 'MainContent';
    ResetPreviousFunction();

    $.ajax({
        url: url,
        type: 'get',
        success: function (res, status, jqXHR) {
            hideProgressBar();
            if (jqXHR.getResponseHeader('content-type').indexOf('text/html')>=0)
            {
                debugger;
                $("#" + targetElement).empty();
                $("#" + targetElement).html(res);

                if (typeof InitializeHeartbeat == 'function') {
                    clearInterval(HeartbeatTimer);
                    InitializeHeartbeat();
                }

                if (typeof PostPageLoad=='function') {
                    try{ 
                        PostPageLoad();
                    }
                    catch(e){
                        console.error('Post Page Load Error' + e)
                    }
                
                }
                else {
                    res = JSON.parse(res);
                    if (res!==null && res.message!==undefined) {
                        ShowAlertMessage(res.message, res.type, res.title);
                        debugger;
                        if (res.PopupUrl != null && res.PopupUrl.length > 0) {
                            window.open(res.PopupUrl, "_blank");
                        }

                        if (res.RedirectUrl !== null && res.RedirectUrl.length>0) {
                            if (res.DoFullRedirect==true) {
                                window.Location = res.RedirectUrl;
                            }
                            else {
                                //its method to call ajax for partial view
                                setTimeout(loadPage, 1000, res.RedirectUrl, "Module");
                            }
                        }
                        else {
                            ShowAlertMessage("no proper response received","error")
                        }
                    }
                }
            }
        },
        error: function () {
            hideProgressBar();
            $("#" + targetElement).html("");
            ShowAlertMessage("An error occured while loading page " + name, "Module", "error");
        }
    });
}


function ShowAlertMessage(message,type,title) {
    if (type!='info' && type!='warning' && type!='success' && type!='error'){
        type == 'info';
        
    }

    $.toast({
        heading: title,
        text: message,
        icon: type,
        ShowHideTransaction: 'fade',
        hideAfter: 3000,
        position: 'top-right'
    });
}

function submitFormData(form) {

    //var Data = $(form).serialize();
    showProgressBar();
    var Data = new FormData($(form)[0]);
    var url = $(form).prop('action');
    debugger;
    var Result = 0;
    $.ajax({
        url: url,
        type: 'post',
        async: false,
        data: Data,
        dataType: 'json',
        cache: false,
        contentType: false,
        processData: false,
        success: function (msg) {
            hideProgressBar();
            debugger;
            ShowAlertMessage(msg.message, msg.MessageClass, msg.MessageTitle);
            Result = msg.ResponseObject;
            debugger;
            if (msg.PopupUrl != null && msg.PopupUrl.length > 0) {
                window.open(msg.PopupUrl, "_blank");
            }

            if (msg.RedirectUrl !== null) {
                if (msg.DoFullRedirect == true) {
                    window.location = msg.RedirectUrl;
                }
                else {
                    setTimeout(loadPage, 1000, msg.RedirectUrl, msg.MessageTitle)
                }
            }
        },
        error: function myfunction() {
            hideProgressBar();
            ShowAlertMessage("An Internal error occured", "error", "Internal error");
        }

    });
    return Result;
}

function ResetPreviousFunction() {
    if(typeof PostPageLoad=='function')
    {
        PostPageLoad == null;
    }

}


function InitializeDateTimePicker(selector) {
    $(selector).datepicker({
        autoClose: true,
        clearBtn: true,
        format: 'dd-M-yyyy',
        todayBtn: true,
        todayHighlight: true,
    });
}

function showProgressBar() {
    $("#preloader").show();
    $("#preloader .preloader").show();
    
}

function hideProgressBar() {
    $("#preloader").hide();
}

function getURLParameters(paramName) {
    var sURL = window.document.URL.toString();
    if (sURL.indexOf("?") > 0) {
        var arrParams = sURL.split("?");
        var arrURLParams = arrParams[1].split("&");
        var arrParamNames = new Array(arrURLParams.length);
        var arrParamValues = new Array(arrURLParams.length);

        var i = 0;
        for (i = 0; i < arrURLParams.length; i++) {
            var sParam = arrURLParams[i].split("=");
            arrParamNames[i] = sParam[0];
            if (sParam[1] != "")
                arrParamValues[i] = unescape(sParam[1]);
            else
                arrParamValues[i] = "No Value";
        }

        for (i = 0; i < arrURLParams.length; i++) {
            if (arrParamNames[i] == paramName) {
                //alert("Parameter:" + arrParamValues[i]);
                return arrParamValues[i];
            }
        }
        return "No Parameters Found";
    }
}

function ClosePopup()
{
    $("#GenericModal").modal('hide');
    $(".modal-backdrop").remove();
}

function OpenPopup(url, title) {

    $("#GenericModalBody").load(url);
    $("#GenericModalTitle").html(title);
    $("#GenericModal").modal('show');
}