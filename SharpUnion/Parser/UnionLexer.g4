lexer grammar UnionLexer;

UNION : U N I O N;
PARAMS : P A R A M S;

IDENTIFIER : (LETTER | UNDERSCORE) (LETTER | UNDERSCORE | DIGIT)*;

OPENBRACKET: '[';
CLOSEBRACKET: ']';
OPENCARET: '<';
CLOSECARET : '>';
OPENPAREN: '(';
CLOSEPAREN : ')';
PIPE : '|';
SEMICOLON : ';';
EQUALS : '=';
COMMA : ',';
QUESTIONMARK : '?';

WS : [ \t];
NEWLINE : '\r' '\n' | [\r\n\u2028\u2029];

fragment UNDERSCORE : '_';
fragment LETTER : [A-Za-z];
fragment DIGIT : [0-9];

fragment A : ('a' | 'A');
fragment B : ('b' | 'B');
fragment C : ('c' | 'C');
fragment D : ('d' | 'D');
fragment E : ('e' | 'E');
fragment F : ('f' | 'F');
fragment G : ('g' | 'G');
fragment H : ('h' | 'H');
fragment I : ('i' | 'I');
fragment J : ('j' | 'J');
fragment K : ('k' | 'K');
fragment L : ('l' | 'L');
fragment M : ('m' | 'M');
fragment N : ('n' | 'N');
fragment O : ('o' | 'O');
fragment P : ('p' | 'P');
fragment Q : ('q' | 'Q');
fragment R : ('r' | 'R');
fragment S : ('s' | 'S');
fragment T : ('t' | 'T');
fragment U : ('u' | 'U');
fragment V : ('v' | 'V');
fragment W : ('w' | 'W');
fragment X : ('x' | 'X');
fragment Y : ('y' | 'Y');
fragment Z : ('z' | 'Z');

ERROR : .;