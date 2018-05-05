module.exports = function(grunt) {
	grunt.initConfig({
		pkg: grunt.file.readJSON('package.json'),
		uglify: {
			options: {
				banner: '/*! <%= pkg.name %> - ((c) <%= pkg.author.name %> <<%= pkg.author.email %>> - <%= pkg.author.url %>) <%= grunt.template.today("yyyy-mm-dd") %> */\n'
			},
			build: {
				src: 'mirrorkey_browser.js',
				dest: 'mirrorkey_browser.min.js'
			}
		}
	});

	grunt.registerTask('browser', function() {
		var contents = grunt.file.read('mirrorkey.js');

		contents = '(function(){\n' + contents;
		contents += '\n}())';

		contents = contents.replace('module.exports =', 'window.mirrorkey =');

		grunt.file.write('mirrorkey_browser.js', contents);
	});

	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.registerTask('default', ['browser', 'uglify']);
};