grammar BoardGame;

options
{
    language=CSharp3;
    output=AST;
}

@namespace { Level14.BoardGameRules }

ID  :   ('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')*
    ;

INT :   '0'..'9'+
    ;

WS  :   ( ' '
        | '\t'
        | '\r'
        | '\n'
        )
        {$channel=Antlr.Runtime.TokenChannels.Hidden;}
    ;

sentence: ID INT EOF;
