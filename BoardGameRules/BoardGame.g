grammar BoardGame;

options {
    language=CSharp3;
    output=AST;
}

tokens {
	LIST;
	LIT_COORDS;
	LIT_INT;
	MOVE_FROM;
	MOVE_OPTIONS;
	MOVE_TO;
	MOVES;
	OP_MOVE;
	PLAYERREF;
	REF;
	SETTINGS;
	STARTINGPIECES;
	STARTINGBOARD;
}

@namespace { Level14.BoardGameRules }


// Lexer rules:

NAME: ('a'..'z'|'A'..'Z') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')* ;

PLACEHOLDER: '_';

INT: '0'..'9'+ ;

WS: ( ' ' | '\t' | '\r' | '\n' )
    {$channel=Antlr.Runtime.TokenChannels.Hidden;} ;

Comment: '#' ~( '\r' | '\n' )*
	{$channel=Antlr.Runtime.TokenChannels.Hidden;} ;


// Parser rules:

int: INT -> ^(LIT_INT INT);

ref: NAME -> ^(REF NAME);

intRef: int | ref;

addExpr: intRef '+' intExpr -> ^('+' intRef intExpr );
subExpr: intRef '-' intExpr -> ^('-' intRef intExpr );

intExpr: intRef | addExpr | subExpr;

intExprP: intExpr | PLACEHOLDER;

coord: '{' intExprP (',' intExprP )* '}' -> ^(LIT_COORDS intExprP+);

namelist_comma: NAME (',' NAME) -> NAME+;

playerRef: 'Player' '(' intExpr ')' -> ^(PLAYERREF intExpr);

settingsRow:
	(
	  NAME ':' int -> ^(NAME int)
	| NAME ':' coord -> ^(NAME coord)
	| NAME ':' namelist_comma -> ^(NAME namelist_comma)
	) ';'
	;

settings: 'Settings' '(' settingsRow+ ')' -> ^(SETTINGS settingsRow+);

pieceStartingCoords: NAME ':' coord ';' -> ^(NAME coord);

startingBoardRow:
	(
		playerRef '(' pieceStartingCoords+ ')' -> ^(STARTINGPIECES playerRef ^(LIST pieceStartingCoords+))
	)
	;

startingBoard: 'StartingBoard' '(' startingBoardRow+ ')' -> ^(STARTINGBOARD startingBoardRow+);

moveOp:
	coord '->' 'Empty'? coord -> ^( OP_MOVE ^(MOVE_FROM coord) ^(MOVE_TO coord) ^(MOVE_OPTIONS 'Empty'?) );

moveRow:
	NAME ':' moveOp ';' -> ^(NAME moveOp);

moves: 'Moves' '(' moveRow+ ')' -> ^(MOVES moveRow+);

sentence: settings startingBoard? moves EOF;
