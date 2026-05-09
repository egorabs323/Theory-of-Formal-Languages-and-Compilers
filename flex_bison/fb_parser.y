%{
#include <stdio.h>
#include "fb_runtime.h"

int yylex(void);
void yyerror(const char* s);
%}

%error-verbose
%locations

%token FINAL DOUBLE IDENTIFIER NUMBER ASSIGN SEMICOLON PLUS MINUS INVALID

%%

input
    : declaration
    ;

declaration
    : FINAL DOUBLE IDENTIFIER ASSIGN value SEMICOLON
    ;

value
    : NUMBER
    | PLUS NUMBER
    | MINUS NUMBER
    ;

%%

void yyerror(const char* s)
{
    const char* fragment = g_last_token_text[0] != '\0' ? g_last_token_text : "EOF";
    int line = yylloc.first_line > 0 ? yylloc.first_line : 1;
    int column = yylloc.first_column > 0 ? yylloc.first_column : 1;
    fb_add_error(fragment, line, column, s);
}
