#ifndef FB_RUNTIME_H
#define FB_RUNTIME_H

#include <stddef.h>

#define FB_MAX_TEXT 256
#define FB_MAX_TOKENS 4096
#define FB_MAX_ERRORS 512

typedef struct FbToken {
    char token_type[32];
    char lexeme[FB_MAX_TEXT];
    int line;
    int column;
    int length;
} FbToken;

typedef struct FbError {
    char fragment[FB_MAX_TEXT];
    int line;
    int column;
    char message[FB_MAX_TEXT];
} FbError;

extern FbToken g_tokens[FB_MAX_TOKENS];
extern int g_token_count;
extern FbError g_errors[FB_MAX_ERRORS];
extern int g_error_count;
extern char g_last_token_text[FB_MAX_TEXT];

void fb_reset_state(void);
void fb_add_token(const char* token_type, const char* lexeme, int line, int column, int length);
void fb_add_error(const char* fragment, int line, int column, const char* message);

#endif
