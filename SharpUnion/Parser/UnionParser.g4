parser grammar UnionParser;

options { tokenVocab = UnionLexer; }

unionStmt : (whitespace | NEWLINE)* UNION whitespace* type whitespace* EQUALS whitespace* member* (whitespace | NEWLINE)* SEMICOLON;

member : NEWLINE whitespace* PIPE whitespace* identifier whitespace* parameterList whitespace*;

parameterList : OPENPAREN whitespace* (parameter whitespace* (COMMA whitespace* parameter)*)? whitespace* CLOSEPAREN;

parameter : (PARAMS whitespace*)? type whitespace* identifier;

type :
    OPENPAREN whitespace* type whitespace* (COMMA whitespace* type)+ whitespace* CLOSEPAREN                               # tupleType
    | identifier whitespace* OPENCARET whitespace* type whitespace* (COMMA whitespace* type)* whitespace* CLOSECARET      # genericType
    | identifier whitespace* OPENBRACKET whitespace* CLOSEBRACKET                                                         # arrayType
    | identifier                                                                                                          # basicType
;

identifier : IDENTIFIER;

whitespace : WS+;