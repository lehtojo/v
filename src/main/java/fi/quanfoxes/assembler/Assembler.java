package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.StringNode;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.parser.nodes.VariableNode;

public class Assembler {
    private static final String SECTION_TEXT = "section .text" + "\n" +
                                               "" + "\n" +
                                               "global _start" + "\n" +
                                               "_start:" + "\n" +
                                               "call function_run" + "\n" +
                                               "" + "\n" +
                                               "mov eax, 1" + "\n" +
                                               "mov ebx, 0" + "\n" +
                                               "int 80h" + "\n\n";

    private static final String SECTION_DATA = "section .data";
    private static final String SECTION_BSS = "section .bss";

    public static String build(Node root, Context context) {
        Builder text = new Builder(SECTION_TEXT);
        Builder data = new Builder(SECTION_DATA);
        Builder bss = Assembler.bss(root);

        Node iterator = root.first();

        while (iterator != null) {
            if (iterator instanceof TypeNode) {
                text.append(Assembler.build((TypeNode)iterator));
            }
            else if (iterator instanceof FunctionNode) {
                text.append(FunctionBuilder.build((FunctionNode)iterator));
            }
            else if (iterator instanceof VariableNode) {
                data.append(Assembler.build((VariableNode)iterator));
            }

            iterator = iterator.next();
        }

        return text +  "\n" + data + "\n" + bss;
    }

    private static Builder bss(Node root) {
        Builder bss = new Builder(SECTION_BSS);
        Assembler.bss(root, bss, 1);
        return bss;
    }

    private static int bss(Node root, Builder builder, int i) {
        Node iterator = root.first();
        
        while (iterator != null) {
            if (iterator instanceof StringNode) {
                String label = "S" + String.valueOf(i++);
                builder.append(Strings.build((StringNode)iterator, label));
            }
            else {
                i = Assembler.bss(iterator, builder, i);
            }

            iterator = iterator.next();
        }

        return i;
    }

    private static String build(TypeNode node) {
        StringBuilder text = new StringBuilder();
        Node iterator = node.first();

        while (iterator != null) {
            if (iterator instanceof TypeNode) {
                text = text.append(Assembler.build((TypeNode)iterator));
            }
            else if (iterator instanceof FunctionNode) {
                text = text.append(FunctionBuilder.build((FunctionNode)iterator));
            }

            iterator = iterator.next();
        }

        return text.toString();
    }

    private static final String DATA = "%s %s 0";

    private static String build(VariableNode node) {
        Variable variable = node.getVariable();
        String operand = Size.get(variable.getType().getSize()).getDataIdentifier();

        return String.format(DATA, variable.getFullname(), operand);
    }
}