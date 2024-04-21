<stmt> -> <decl-stmt> |
		  <init-stmt> |
		  <expr-stmt>
<decl-stmt> -> <attribute><init><init-list-rest>;
<attribute> -> int
<init-stmt> -> <init><init-list-rest>;
<init> -> <identifier> |
		  <identifier> = <expr>
<init-list-rest> -> ,<init><init-list-rest> |
					<empty>
<expr-stmt> -> <function>;
<expr> -> <simple-expr><operation><simple-expr> |
		  <simple-expr>
<simple-expr> -> <string> |
				 <digit> |
				 <identifier> |
				 <func>
<operation> -> + |
			   - |
			   * |
			   / 
<func> -> <identifier>(<args>)
<args> -> <expr><args-list-rest> |
		  <empty>
<args-list-rest> -> ,<expr><args-list-rest> |
					<empty>
<identifier> -> <letter><alphanumeric-list>
<alphanumeric-list> -> (<letter> | <digit>)<alphanumeric-list> |
					   <empty>
<number> -> <digit><number-rest>
<number-rest> -> <number><number-rest> |
				 <empty>
<letter> -> a-z | A-Z 
<digit> -> 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 0