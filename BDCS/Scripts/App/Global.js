var HeartbeatTimer;

// Lightweight page-history stack for the top "Back" button. Only top-level page
// navigations (targetElement === 'MainContent') are tracked - tab switches, header
// refreshes, and modal loads target a different element and are intentionally ignored,
// so "Back" always means "the previous full page", not "the last thing that changed".
var NavigationHistory = [];
var CurrentPage = null;

function loadPage(url, name, targetElement, isBackNavigation, skeletonHtml) {
    targetElement = targetElement || 'MainContent';

    //a shimmering skeleton in the exact area being replaced, instead of the old
    //full-page blank-white-plus-spinner overlay - every AJAX navigation in the app
    //goes through here (patient list, admin screens, vital history tabs, lab
    //orders, protocols, etc.), so this single change covers all of them. A caller
    //that already knows the real layout (e.g. Vital History's own 3-column card
    //grid) can pass its own skeletonHtml instead of the generic one.
    if (skeletonHtml) {
        $("#" + targetElement).html('<div class="admin-module"><div class="am-skeleton-wrap">' + skeletonHtml + '</div></div>');
    } else {
        ShowGenericSkeleton(targetElement);
    }

    ResetPreviousFunction();

    $.ajax({
        url: url,
        type: 'get',
        success: function (res, status, jqXHR) {
            hideProgressBar();
            if (jqXHR.getResponseHeader('content-type').indexOf('text/html') >= 0) {

                if (targetElement === 'MainContent') {
                    if (!isBackNavigation && CurrentPage) {
                        NavigationHistory.push(CurrentPage);
                    }
                    CurrentPage = { url: url, name: name };
                }

                $("#" + targetElement).empty();
                $("#" + targetElement).html(res);

                if (typeof InitializeHeartbeat == 'function') {
                    clearInterval(HeartbeatTimer);
                    InitializeHeartbeat();
                }

                if (typeof PostPageLoad == 'function') {
                    try {
                        PostPageLoad();
                    }
                    catch (e) {
                        console.error('Post Page Load Error' + e)
                    }

                }
                else {
                    res = JSON.parse(res);
                    if (res !== null && res.message !== undefined) {
                        ShowAlertMessage(res.message, res.type, res.title);

                        if (res.PopupUrl != null && res.PopupUrl.length > 0) {
                            window.open(res.PopupUrl, "_blank");
                        }

                        if (res.RedirectUrl !== null && res.RedirectUrl.length > 0) {
                            if (res.DoFullRedirect == true) {
                                window.Location = res.RedirectUrl;
                            }
                            else {
                                //its method to call ajax for partial view
                                setTimeout(loadPage, 1000, res.RedirectUrl, "Module");
                            }
                        }
                        else {
                            ShowAlertMessage("no proper response received", "error")
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

function GoBackPage() {
    // Prefer the in-app AJAX history (Patient/Visit screens, which never change the
    // browser URL) when there's an entry; the Admin screens (Users/Roles/MDT Types/
    // Locations/Lookups) use plain full-page links instead, so they never populate
    // this stack - the browser's own history already has a real entry for those,
    // so falling back to it there is what makes Back work everywhere, always.
    if (NavigationHistory.length > 0) {
        var previous = NavigationHistory.pop();
        loadPage(previous.url, previous.name, 'MainContent', true);
    } else {
        window.history.back();
    }
}


function ShowAlertMessage(message, type, title) {
    if (type != 'info' && type != 'warning' && type != 'success' && type != 'error') {
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

            ShowAlertMessage(msg.message, msg.MessageClass, msg.MessageTitle);
            Result = msg.ResponseObject;

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
    if (typeof PostPageLoad == 'function') {
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

//Generic shimmering placeholder used by loadPage() for every AJAX navigation -
//deliberately content-agnostic (a handful of card-shaped blocks) rather than
//trying to mimic each target screen's real layout, which varies far too much
//(tables, forms, card grids) for one generic skeleton to match exactly. Wrapped
//in its own .admin-module div since the target container (e.g. #MainContent)
//isn't always already inside one, and the skeleton's CSS (admin-module.css) is
//scoped under that class.
function GenericSkeletonCardHtml(lineWidths) {
    var lines = lineWidths.map(function (w) {
        return '<div class="am-skeleton-line" style="width:' + w + '"></div>';
    }).join('');
    return '<div class="am-skeleton-card"><div class="am-skeleton-card-head"></div><div class="am-skeleton-card-body">' + lines + '</div></div>';
}

function ShowGenericSkeleton(targetElement) {
    var target = document.getElementById(targetElement);
    if (!target) {
        return;
    }

    var cards = [
        ["40%", "75%", "60%"],
        ["55%", "90%"],
        ["35%", "80%", "65%"]
    ].map(GenericSkeletonCardHtml).join('');

    target.innerHTML = '<div class="admin-module"><div class="am-skeleton-wrap">' + cards + '</div></div>';
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

function ClosePopup() {
    $("#GenericModal").modal('hide');
    $(".modal-backdrop").remove();
}

function OpenPopup(url, title) {

    $("#GenericModalBody").load(url);
    $("#GenericModalTitle").html(title);
    $("#GenericModal").modal('show');
}