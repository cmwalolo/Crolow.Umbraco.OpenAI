angular.module("umbraco").controller("CrolowAIController", function ($scope, crolowAIResource, languageResource) {
    var vm = this;

    init();

    function init() {
        vm.loadiNg = false;
        vm.recursive = 1;
        vm.function = -1;
        vm.role = "";

        vm.functions = [
            { id: 0, value: "Describe Image" },
            { id: 1, value: "Correct" },
            { id: 2, value: "Summarize" },
            { id: 3, value: "Translate" },
            { id: 4, value: "Create Hints" }
        ];

        vm.contentPickerProperty = {
            alias: "myProperty",
            label: "My property",
            description: "Select a node to start with",
            value: "",
            config: {
                startNodeId: -1
            },
            view: "contentpicker"
        };
        languageResource.getAll().then(function (languages) {
            vm.languages = languages;
        });

        crolowAIResource.getConfig().then(function (data) {
            vm.config = data;
        });
    };

    vm.reset = function () {
        vm.loading = true;

        var functionName = vm.functions[vm.function].value.replace(/ /g, '');
        var model =
        {
            nodeId: vm.contentPickerProperty.value,
            sourceLanguage: vm.sourceLanguage,
            targetLanguage: vm.targetLanguage,
            recursive: vm.recursive,
            role: vm.role
        };

        crolowAIResource.execute(functionName, model).then(function (response) {
            vm.loading = false;
            vm.data = response;
        });

    };

    vm.changeFunction = function () {
        switch (vm.function) {
            case 0:
                vm.roles = vm.config.DescribeImages.Roles;
                break;
            case 1:
                vm.roles = vm.config.Corrections.Roles;
                break;
            case 2:
                vm.roles = vm.config.Summaries.Roles;
                break;
            case 3:
                vm.roles = vm.config.Translations.Roles;
                break;
        }
    }

});