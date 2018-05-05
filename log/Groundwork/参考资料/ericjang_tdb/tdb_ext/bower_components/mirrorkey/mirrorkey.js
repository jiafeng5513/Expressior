'use strict';

var isArray;
if (typeof Array.isArray === 'undefined') {
	isArray = function (arg) {
		return Object.prototype.toString.call(arg) === '[object Array]';
	};
} else {
	isArray = Array.isArray;
}

var transforms = {
	'none': function(key) {
		return key;
	},

	'camel-case': function(key) {
		var parts = key.toLowerCase().split('_');
		var part;

		for (var idx = 0, len = parts.length; idx < len; idx++) {
			part = parts[idx];
			part = part.substr(0, 1).toUpperCase() + part.substr(1);

			parts[idx] = part;
		}

		return parts.join('');
	},

	'lower-case': function(key) {
		return key.toLowerCase();
	},

	'dashed': function(key) {
		return key.replace(/_/g, '-');
	},

	'lower-dashed': function(key) {
		return this['lower-case'](this['dashed'](key));
	}
};

var transformNames = [];
for (var transformName in transforms) {
	transformNames.push(transformName);
}

module.exports = function(obj, transformType) {
	var objIsArray = isArray(obj);

	if (obj === null || (typeof obj !== 'object' && objIsArray === false)) {
		throw 'The first argument to mirrorKey must be a object or an array.';
	}

	if (typeof transformType === 'undefined') {
		transformType = 'none';
	} else {
		if (transformNames.indexOf(transformType) === -1) {
			throw 'Unknown value for transformType. Valid values: ' + transformNames.join(', ');
		}
	}

	var result = {};

	if (objIsArray === false) {
		for (var key in obj) {
			if (obj.hasOwnProperty(key) === false) {
				continue;
			}

			result[key] = transforms[transformType](key);
		}
	} else {
		for (var idx = 0, len = obj.length; idx < len; idx++) {
			result[obj[idx]] = transforms[transformType](obj[idx]);
		}
	}

	return result;
};