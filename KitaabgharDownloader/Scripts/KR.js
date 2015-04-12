$(function () {
    $(document).ready(function () {
        $(".progress").hide();
    });

    var hub = $.connection.downloaderHub,
        $status = $("#statusItem"),
        $progress = $("#progressItem"),
        $link = $("#linkItem");
    
    $.extend(hub.client, {
        statusChanged: function (statusMsg) {
            $status.append("<li class=\"list-group-item\">" + statusMsg + "</li>");
        },
        progressChanged: function (percent) {
            $progress.css.width = percent+ "% ";
        }

    });

    $.connection.hub.start().done(function() {
        $link.keydown(function(e) {
            if (e.keyCode === 13) {
                console.log("Connected")
                //$.connection.hub.startDownload($link.val());
                hub.server.startDownload($link.val());
                $(".progress").show();
            };
        });
    });
});