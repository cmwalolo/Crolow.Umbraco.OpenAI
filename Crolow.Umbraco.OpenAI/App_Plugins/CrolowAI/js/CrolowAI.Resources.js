// adds the resource to umbraco.resources module:
angular.module('umbraco.resources').factory('crolowAIResource', function ($q, $http, umbRequestHelper) {
    return {
        execute: function (functionName, config) {
            return umbRequestHelper.resourcePromise($http.post("/umbraco/backoffice/Api/CrolowAIApi/" + functionName, config), "Failed to retrieve comments");
        },
        getConfig: function () {
            return umbRequestHelper.resourcePromise($http.post("/umbraco/backoffice/Api/CrolowAIApi/GetConfig"), "Failed to execute the request");
        }
    };
});