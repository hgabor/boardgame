grammar BoardGame;

options {
    language=CSharp3;
    output=AST;
}

tokens {
	SETTINGS;
}

@namespace { Level14.BoardGameRules }

NAME: ('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')* ;

INT: '0'..'9'+ ;

coord: '{' INT (',' INT)* '}' -> INT+;

namelist_comma: NAME (',' NAME) -> NAME+;

settingsRow:
	(
	  NAME ':' INT -> ^(NAME INT)
	| NAME ':' coord -> ^(NAME coord)
	| NAME ':' namelist_comma -> ^(NAME namelist_comma)
	) ';'
	;

settings: 'Settings' '(' settingsRow+ ')' -> ^(SETTINGS settingsRow+);


WS: ( ' ' | '\t' | '\r' | '\n' )
    {$channel=Antlr.Runtime.TokenChannels.Hidden;} ;

sentence: settings EOF;
