# Interpreter Program

## Overview

The Interpreter has three main components: lexical analysis (Lexer), syntactic analysis (Parser), and code execution (Executor). These components are passed to the Interpreter class via interfaces.

## Components

1. **Lexer (ILexer):** This component tokenizes the input code, breaking it down into individual elements such as keywords, identifiers, and symbols.

2. **Parser (IParser):** The Parser component analyzes the structure of the tokenized code to determine its syntactic correctness and generate.

3. **Executor (IExecutor):** The Executor component takes tree produced by the Parser and executes it. It's constructed with interfaces for input (IRead) and output (IWrite), allowing to determine behavior of Read and Write functions.

## Parser tree structure
1. \<stmt> -> \<decl-stmt> |
			  \<init-stmt> |
			  \<func-stmt>

2. \<decl-stmt> -> \<attribute>\<init>\<init-list-rest>;

3. \<attribute> -> int

4. \<init-stmt> -> \<init>\<init-list-rest>;

5. \<init> -> \<identifier> |
		      \<identifier> = \<expr>

6. \<init-list-rest> -> ,\<init>\<init-list-rest> |
					    \<empty>

7. \<func-stmt> -> \<function>;

8. \<expr> -> \<simple-expr>\<operation>\<simple-expr> |
		      \<simple-expr>

9. \<simple-expr> -> \<string> |
				     \<digit> |
				     \<identifier> |
				     \<func>

10. \<operation> -> + |
			 	    - |
			 	    * |
			 		/ 

11. \<func> -> \<identifier>(\<args>)

12. \<args> -> \<expr>\<args-list-rest> |
			   \<empty>

13. \<args-list-rest> -> ,\<expr>\<args-list-rest> |
						 \<empty>

14. \<identifier> -> \<letter>\<alphanumeric-list>

15. \<alphanumeric-list> -> (\<letter> | \<digit>)\<alphanumeric-list> |
							\<empty>

16. \<number> -> \<digit>\<number-rest>

17. \<number-rest> -> \<number>\<number-rest> |
					  \<empty>

18. \<letter> -> a-z | A-Z 

19. \<digit> -> 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 0

## Interpreter errors
### Lexer errors
	1. Unexpected character
	2. Not closed string
	3. \n in string

### Parser errors:
	all cases when the structure of the program does not match the tree above

### Executer errors:
	1. Redefinition variable
	2. Not declared variable
	3. Uknown function
	4. Invalid function arguments