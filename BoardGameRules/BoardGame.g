grammar BoardGame;

options {
    language=CSharp3;
    output=AST;
    backtrack=true;
}

// Imaginary tokens
tokens {
    ASSIGNMENT;
    EVENT;
    EVENTS;
    EVENTTYPE;
    EVENTTYPES;
    FUNCCALL;
    FUNCDEF;
    FUNCDEFLIST;
    FUNCNAME;
    IF;
    IF_CONDITION;
    IF_ACTION;
    INIT;
    LIST;
    LIT_COORDS;
    LIT_INT;
    LIT_SET;
    MEMBER_ACCESS;
    MOVE_FROM;
    MOVE_OPTIONS;
    MOVE_TO;
    MOVES;
    ONLY_MODIFIER;
    OP_MOVE;
    PARAMLIST;
    PLAYERREF;
    REF;
    SELECT;
    SELECT_FROM;
    SELECT_WHERE;
    SETTINGS;
    STARTINGPIECES;
    STARTINGBOARD;
    STATEMENTS;
    TAG;
    VAR_REF;
    VAR_MEMBER_ACCESS;
}

@namespace { Level14.BoardGameRules }


// Lexer rules
// ===========

OP_ADD: '+';
OP_AND: 'And';
OP_ASSIGNMENT: ':=';
OP_DIV: '/';
OP_EQ: '=';
OP_GT: '>';
OP_GTE: '>=';
OP_LT: '<';
OP_LTE: '<=';
OP_MOD: '%';
OP_MUL: '*';
OP_NE: '!=';
OP_NOT: 'Not';
OP_OR: 'Or';
OP_SUB: '-';

// Special name that can appear where variables can too
OFFBOARD: 'Offboard';

NAME: ('a'..'z'|'A'..'Z'|'$') ('a'..'z'|'A'..'Z'|'0'..'9'|'$'|'_')* ;

PLACEHOLDER: '_';

INT: '0'..'9'+ ;


// Whitespaces and comments are irrelevant
// Full qualification for "Antlr.Runtime.TokenChannels.Hidden" is needed because of C# errors
WS: ( ' ' | '\t' | '\r' | '\n' )
    {$channel=Antlr.Runtime.TokenChannels.Hidden;} ;

Comment: '#' ~( '\r' | '\n' )*
	{$channel=Antlr.Runtime.TokenChannels.Hidden;} ;


// General parser rules
// ====================

// Int
int: INT -> ^(LIT_INT INT);
intRef: int | ref;

// Coords
coord: '{' placeholderExpr (',' placeholderExpr )* '}' -> ^(LIT_COORDS placeholderExpr+);
placeholderExpr: '_' | expr;

// Player
playerRef: 'Player' '(' expr ')' -> ^(PLAYERREF expr);

// Set
selectExpr:
    '[' 'Select' name 'From' ref ']' -> ^(SELECT name ^(SELECT_FROM ref)) |
    '[' 'Select' name 'From' ref 'Where' expr ']' -> ^(SELECT name ^(SELECT_FROM ref) ^(SELECT_WHERE expr)) ;
setLiteral: '[' (expr (',' expr)* ','?)? ']' -> ^(LIT_SET expr+);
setExpr: selectExpr | setLiteral;

// Basic variable references
name:
    OFFBOARD -> ^(REF OFFBOARD) |
    NAME -> ^(REF NAME);
varName:
    NAME -> ^(VAR_REF NAME);
ref:
    playerRef
    | namedFunc
    | name;
namedFunc: name '(' (expr (',' expr)*)? ')' -> ^(FUNCCALL ^(FUNCNAME name) ^(LIST expr*));
nameOrFunction: namedFunc | name;

// Operators, including precedence:
expr: andExpr (OP_OR^ andExpr)*;
andExpr: eqExpr (OP_AND^ eqExpr)*;
eqExpr: relExpr ((OP_EQ|OP_NE)^ relExpr)*;
relExpr: addExpr ((OP_GT|OP_LT|OP_GTE|OP_LTE)^ addExpr)*;
addExpr: mulExpr ((OP_ADD|OP_SUB)^ mulExpr)*;
mulExpr: unaryExpr ((OP_MUL|OP_DIV|OP_MOD)^ unaryExpr)*;
unaryExpr:
    OP_SUB memberExpr -> ^(OP_SUB memberExpr) |
    OP_ADD? memberExpr -> memberExpr |
    OP_NOT memberExpr -> ^(OP_NOT memberExpr);
memberExpr:
    primeExpr ('.' nameOrFunction)+ -> ^(MEMBER_ACCESS primeExpr nameOrFunction*) |
    primeExpr;
primeExpr: int | coord | ref | setExpr | '(' expr ')' -> expr;

// Member acces operator special cases
varMemberExpr:
    primeExpr ('.' nameOrFunction)* '.' varName -> ^(MEMBER_ACCESS primeExpr nameOrFunction varName);

// Statements
statement:
    assignmentStatement ';' -> assignmentStatement |
    returnStatement ';' -> returnStatement |
    methodStatement ';' -> methodStatement |
    ifBlock |
    namedFunc ';' -> namedFunc ;
assignmentStatement:
    varMemberExpr OP_ASSIGNMENT expr -> ^(ASSIGNMENT varMemberExpr expr) |
    varName OP_ASSIGNMENT expr -> ^(ASSIGNMENT varName expr);
returnStatement:
    'Return' expr -> ^('Return' expr);
methodStatement:
    primeExpr ('.' nameOrFunction)* '.' namedFunc -> ^(MEMBER_ACCESS primeExpr nameOrFunction* namedFunc);
ifBlock:
    'If' expr 'Then' statement+ 'End' -> ^(IF ^(IF_CONDITION expr) ^(IF_ACTION statement+) );


// Rulebook parts
// ==============

// Settings block
settings: 'Settings' '(' settingsRow+ ')' -> ^(SETTINGS settingsRow+);
settingsRow:
    (
      NAME ':' int -> ^(NAME int)
    | NAME ':' coord -> ^(NAME coord)
    | NAME ':' NAME (',' NAME)* -> ^(NAME NAME+)
    ) ';'
    ;

// Functions block
functionBlock:
    'Functions' '(' funcDef+ ')' -> ^(FUNCDEFLIST funcDef+);
funcDef:
    name '(' ( NAME (',' NAME)*)? ')' '(' statement+ ')' -> ^(FUNCDEF name ^(PARAMLIST NAME*) ^(STATEMENTS statement+));

// Init block
init: 'Init' '(' statement+ ')' -> ^(INIT ^(STATEMENTS statement+));

// StartingBoard block
startingBoard: 'StartingBoard' '(' invalidBoardRow? startingBoardRow+ ')' -> ^(STARTINGBOARD invalidBoardRow? startingBoardRow+);
invalidBoardRow:
    (
        'Valid' '(' (expr ';')+ ')' -> ^('Valid' expr+ ) |
        'Invalid' '(' (expr ';')+ ')' -> ^('Invalid' expr+ )
    );
startingBoardRow:
    (
        playerRef '(' pieceStartingCoords+ ')' -> ^(STARTINGPIECES playerRef ^(LIST pieceStartingCoords+))
    );
pieceStartingCoords:
    NAME ':' 'Offboard' ';' -> ^(NAME 'Offboard') |
    NAME ':' coord ';' -> ^(NAME coord) |
    NAME NAME ':' 'Offboard' ';' -> ^(NAME 'Offboard' ^(TAG NAME)) |
    NAME NAME ':' coord ';' -> ^(NAME coord ^(TAG NAME)) ;
coordOffboard: coord | OFFBOARD;

// Moves block
moves: 'Moves' '(' moveRow+ ')' -> ^(MOVES moveRow+);
moveRow:
    NAME ':' moveOp ';' -> ^(NAME moveOp);
moveOp:
    from=coordOffboard '->' 'Empty'? to=coordOffboard moveLabel? moveIf? moveThen?
    ->
    ^( OP_MOVE ^(MOVE_FROM $from) ^(MOVE_TO $to) ^(MOVE_OPTIONS 'Empty'?) moveLabel? moveIf? moveThen? );
moveLabel: 'Label' NAME -> ^('Label' NAME);
moveIf: 'If' expr -> ^(IF expr);
moveThen:
    'Then' statement+ 'End' -> ^(STATEMENTS statement+);

// Events block
events: 'Events' '(' event+ ')' -> ^(EVENTS event+);
event: 'Only'? eventType ( ',' eventType )* '(' statement+ ')' -> ^(EVENT ^(EVENTTYPES eventType+)  ^(STATEMENTS statement+) 'Only'?);
eventType: playerRef '.' NAME -> ^(EVENTTYPE playerRef NAME);

// Root parser rule
sentence: settings functionBlock? init? startingBoard? moves events EOF;
