/** @type {import('prettier').Config} */
export default {
	endOfLine: 'auto',
	printWidth: 100,
	useTabs: true,
	singleQuote: true,
	proseWrap: 'always',
	overrides: [
		{
			files: ['.vscode/*.json'],
			options: {
				parser: 'json5',
				trailingComma: 'all',
				quoteProps: 'preserve',
				singleQuote: false,
			},
		},
	],
};
