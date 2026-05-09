%option noyywrap nounput noinput

%{
#include <string.h>
#include "fb_parser.tab.h"
#include "fb_runtime.h"

static int fb_line = 1;
static int fb_col = 1;

static void fb_update_location(const char* text, int len)
{
    int i;
    yylloc.first_line = fb_line;
    yylloc.first_column = fb_col;

    for (i = 0; i < len; i++)
    {
        if (text[i] == '\n')
        {
            fb_line++;
            fb_col = 1;
        }
        else
        {
            fb_col++;
        }
    }

    yylloc.last_line = fb_line;
    yylloc.last_column = fb_col - 1;
}

#define YY_USER_ACTION fb_update_location(yytext, yyleng);
%}

DIGIT [0-9]
ID [A-Za-z_][A-Za-z0-9_]*
EXP [eE][+-]?{DIGIT}+
NUMBER {DIGIT}+\.{DIGIT}+({EXP})?
WS [ \t\r\n]+

%%

"final" {
    fb_add_token("Keyword", yytext, yylloc.first_line, yylloc.first_column, yyleng);
    return FINAL;
}

"double" {
    fb_add_token("Keyword", yytext, yylloc.first_line, yylloc.first_column, yyleng);
    return DOUBLE;
}

"=" {
    fb_add_token("Operator", yytext, yylloc.first_line, yylloc.first_column, yyleng);
    return ASSIGN;
}

";" {
    fb_add_token("Separator", yytext, yylloc.first_line, yylloc.first_column, yyleng);
    return SEMICOLON;
}

"+" {
    fb_add_token("Operator", yytext, yylloc.first_line, yylloc.first_column, yyleng);
    return PLUS;
}

"-" {
    fb_add_token("Operator", yytext, yylloc.first_line, yylloc.first_column, yyleng);
    return MINUS;
}

{NUMBER} {
    fb_add_token("NumberLiteral", yytext, yylloc.first_line, yylloc.first_column, yyleng);
    return NUMBER;
}

{ID} {
    fb_add_token("Identifier", yytext, yylloc.first_line, yylloc.first_column, yyleng);
    return IDENTIFIER;
}

{WS} {
    fb_add_token("Whitespace", yytext, yylloc.first_line, yylloc.first_column, yyleng);
}

. {
    fb_add_token("Error", yytext, yylloc.first_line, yylloc.first_column, yyleng);
    return INVALID;
}

%%
