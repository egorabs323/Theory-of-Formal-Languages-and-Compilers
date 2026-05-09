#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "fb_runtime.h"

typedef struct yy_buffer_state* YY_BUFFER_STATE;

int yyparse(void);
YY_BUFFER_STATE yy_scan_string(const char* str);
void yy_delete_buffer(YY_BUFFER_STATE buffer);
void yy_switch_to_buffer(YY_BUFFER_STATE new_buffer);
void yyrestart(FILE* input_file);

FbToken g_tokens[FB_MAX_TOKENS];
int g_token_count = 0;
FbError g_errors[FB_MAX_ERRORS];
int g_error_count = 0;
char g_last_token_text[FB_MAX_TEXT];

static void fb_copy_text(char* target, const char* source, size_t target_size)
{
    if (target == NULL || target_size == 0)
    {
        return;
    }

    if (source == NULL)
    {
        target[0] = '\0';
        return;
    }

    strncpy(target, source, target_size - 1);
    target[target_size - 1] = '\0';
}

void fb_reset_state(void)
{
    g_token_count = 0;
    g_error_count = 0;
    g_last_token_text[0] = '\0';
}

void fb_add_token(const char* token_type, const char* lexeme, int line, int column, int length)
{
    if (g_token_count >= FB_MAX_TOKENS)
    {
        return;
    }

    fb_copy_text(g_last_token_text, lexeme, sizeof(g_last_token_text));

    fb_copy_text(g_tokens[g_token_count].token_type, token_type, sizeof(g_tokens[g_token_count].token_type));
    fb_copy_text(g_tokens[g_token_count].lexeme, lexeme, sizeof(g_tokens[g_token_count].lexeme));
    g_tokens[g_token_count].line = line;
    g_tokens[g_token_count].column = column;
    g_tokens[g_token_count].length = length;
    g_token_count++;
}

void fb_add_error(const char* fragment, int line, int column, const char* message)
{
    if (g_error_count >= FB_MAX_ERRORS)
    {
        return;
    }

    fb_copy_text(g_errors[g_error_count].fragment, fragment, sizeof(g_errors[g_error_count].fragment));
    fb_copy_text(g_errors[g_error_count].message, message, sizeof(g_errors[g_error_count].message));
    g_errors[g_error_count].line = line;
    g_errors[g_error_count].column = column;
    g_error_count++;
}

static void fb_print_escaped(const char* text)
{
    const char* cursor = text;
    while (*cursor != '\0')
    {
        if (*cursor == '\\')
        {
            fputs("\\\\", stdout);
        }
        else if (*cursor == '\n')
        {
            fputs("\\n", stdout);
        }
        else if (*cursor == '\r')
        {
            fputs("\\r", stdout);
        }
        else if (*cursor == '\t')
        {
            fputs("\\t", stdout);
        }
        else if (*cursor == '|')
        {
            fputs("\\|", stdout);
        }
        else
        {
            fputc(*cursor, stdout);
        }
        cursor++;
    }
}

static void fb_print_output(void)
{
    int i;

    for (i = 0; i < g_token_count; i++)
    {
        printf("TOKEN|%s|", g_tokens[i].token_type);
        fb_print_escaped(g_tokens[i].lexeme);
        printf("|%d|%d|%d\n", g_tokens[i].line, g_tokens[i].column, g_tokens[i].length);
    }

    for (i = 0; i < g_error_count; i++)
    {
        printf("ERROR|");
        fb_print_escaped(g_errors[i].fragment);
        printf("|%d|%d|", g_errors[i].line, g_errors[i].column);
        fb_print_escaped(g_errors[i].message);
        printf("\n");
    }

    if (g_error_count == 0)
    {
        printf("STATUS|OK\n");
    }
    else
    {
        printf("STATUS|FAIL\n");
    }
}

int main(int argc, char** argv)
{
    FILE* input_file;
    long file_size;
    size_t read_count;
    char* buffer;
    YY_BUFFER_STATE state;

    if (argc < 2)
    {
        fprintf(stderr, "Usage: fb_analyzer <input-file>\n");
        return 2;
    }

    input_file = fopen(argv[1], "rb");
    if (input_file == NULL)
    {
        fprintf(stderr, "Cannot open input file\n");
        return 2;
    }

    if (fseek(input_file, 0, SEEK_END) != 0)
    {
        fclose(input_file);
        return 2;
    }

    file_size = ftell(input_file);
    if (file_size < 0)
    {
        fclose(input_file);
        return 2;
    }

    if (fseek(input_file, 0, SEEK_SET) != 0)
    {
        fclose(input_file);
        return 2;
    }

    buffer = (char*)malloc((size_t)file_size + 1);
    if (buffer == NULL)
    {
        fclose(input_file);
        return 2;
    }

    read_count = fread(buffer, 1, (size_t)file_size, input_file);
    fclose(input_file);
    buffer[read_count] = '\0';

    fb_reset_state();
    yyrestart(NULL);
    state = yy_scan_string(buffer);
    yyparse();
    yy_delete_buffer(state);

    fb_print_output();

    free(buffer);
    return 0;
}
