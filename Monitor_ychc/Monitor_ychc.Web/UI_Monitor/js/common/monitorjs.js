$(function () {
    InitializePage();
});
var publicData = {
    realTimer: {},
    pollingIntervals: 10000
};

function InitializePage() {
    loadTemplate("template.html");
    setTimeout(getData, 1000);
}
function getData() {
    getLatestData();
}
function loadTemplate(url) {
    // 将模板加载至 templatePlaceHolder
    $("#template").load(url);
}
function getLatestData() {
    //var m_MsgData;
    var dataToServer = {
        organizationId: pageData.organizationId,
        sceneName: pageData.viewName
    };
    var urlString = "MonitorView.aspx/GetRealTimeData";
    $.ajax({
        type: "POST",
        url: urlString,
        data: JSON.stringify(dataToServer),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            serviceSuccessful(data);
        }
    });
}
function serviceSuccessful(resultObject) {
    displayScene(resultObject.d);
    setupTimerToPollLatestData();
}
function setupTimerToPollLatestData() {
    // 设置获取最新数据定时器
    clearTimeout(publicData.realTimer);
    publicData.realTimer = setTimeout(
        function () {
            getLatestData();
        }, publicData.pollingIntervals);
}
function displayScene(scene) {
    // 显示监控画面参数
    // $("#sceneName").html(scene.Name);
    var datetime = $.jsonDateToDateTime(scene.time);
    $("#timestamp").html(datetime);

    // 显示数据项
    displayDataItem(scene.DataSet);
}
function displayDataItem(dataSets) {
    $.each(dataSets, function (i, item) {
        var value = Number(item.Value)
        $(document.getElementById(item.ID)).html(value.toFixed(0));
    });
}