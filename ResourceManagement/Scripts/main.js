jQuery(document).ready(function () {
    FindAncherTagInHtml();

    jQuery(".dropdownheading").click(function () {
        jQuery(".agent-description").toggleClass("show");
        jQuery(this).toggleClass("active");
    });

    //This logic to handle out side clik of agent popup
    jQuery(document).on('click', function (e) {
        if (jQuery(e.target).closest(".agent-navigation").length === 0) {
            jQuery(".agent-description").hide();
        }

        if (jQuery(e.target).closest(".agent-navigation").length > 0 && jQuery(".agent-description.show").length > 0) {
            jQuery(".agent-description").show();
        }
        else {
            jQuery(".agent-description").hide();
            jQuery(".dropdownheading").removeClass("active");
            jQuery(".agent-description").removeClass("show");
        }
    });

    jQuery('.noagent-search input').val('');

    jQuery('.noagent-search input').on('keyup', function () {
        var self = jQuery(this);
        if (self.val() != '') {
            jQuery(this).addClass('is-not-empty');
        } else {
            jQuery(this).removeClass('is-not-empty');
        }
    });

    jQuery('.noagent-search input').on('keypress', function (e) {
        var inputVal = jQuery(this).val();
        var reqVal = "/de-de/vor-ort?newsearch=true&navSearchTerm=" + inputVal;
        if (inputVal != '' && e.which == 13) {
            window.location = reqVal;
            return false;
        }
    });



    // quick links js
    function HideFlyout() {
        jQuery('.main-toggle-flyout').removeClass('active');
        jQuery('.main-toggle-flyout').find('.column-splitter').removeClass('show');
    }

    function FindAncherTagInHtml() {
        var findAncherTag = $('.dropdown-flyout').find('a');
        var separator = ".";
        $.each(findAncherTag, function (key, value) {
            var findClassName = findAncherTag[key].closest('.main-toggle-flyout').className;
            findClassName = separator + findClassName.replace(/ /g, separator);
            if (jQuery(findClassName).find('a').attr('href')) {
                jQuery(findClassName).addClass('removecss-mobile');
            }
        });
    }

    jQuery(".dropdown-flyout").click(function (e) {
        var mainContentDiv = jQuery(this).parent().parent();
        if (mainContentDiv.hasClass('active')) {
            HideFlyout();
        }
        else {
            if (!$(this).find('a').attr('href')) {
                HideFlyout();
                mainContentDiv.find(".column-splitter").toggleClass("show");
                mainContentDiv.toggleClass("active");
            }
        }
    });
    jQuery(document).on('click', function (e) {
        if (jQuery(e.target).closest(".main-toggle-flyout").length === 0) {
            HideFlyout();
        }
    });


    jQuery(document).ajaxSend(function () {
        jQuery("#overlay").fadeIn(300);
    });

    jQuery(document).ajaxComplete(function (event, request, set) {
        jQuery("#overlay").fadeOut(300);
    });
});

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

jQuery(document).ready(function () {
    if (window.location.href.toString().toLowerCase().indexOf("?success") > -1) {
        var responseSuccess = getUrlVars()["success"];
        var selectedEmp = decodeURIComponent(getUrlVars()["selectedemp"]);     

        if (responseSuccess == "true") {
            jQuery('.success-message-popup-title').html("Hey there");
            jQuery('.database-response').html("<h2><b> " + selectedEmp + "</b> details have been successfully saved, and we greatly appreciate your cooperation.</h2>");
            jQuery('.message-success-popup').modal('show');
        }
        else {
            var responseError = getUrlVars()["error"];
            var decodedUrlText = decodeURIComponent(responseError);
            jQuery('.error-message-popup-title').html("Hey there");
            jQuery('.database-response').html("<h2>" + decodedUrlText + "</h2>");
            jQuery('#modal-db-error-alert').modal('show');
        }

        var yourCurrentUrl = window.location.href.split('?')[0];
        window.history.replaceState({}, '', yourCurrentUrl);
    }

});


jQuery('textarea').on('input', function () {
    this.style.height = 'auto';

    this.style.height =
        (this.scrollHeight) + 'px';
});


