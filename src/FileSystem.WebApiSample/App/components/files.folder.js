(function() {
    angular.module('app.files')
        .directive('folder', folder);

    folder.$inject = [];

    function folder() {
        var directive = {
            link: link,
            templateUrl: '/App/components/files.folder.html',
            restrict: 'A'
        };

        return directive;

        function link(scope, element, attrs) {
            /* */
        }
    }
})();