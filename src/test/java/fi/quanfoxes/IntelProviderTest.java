package fi.quanfoxes;

import fi.quanfoxes.Lexer.IdentifierToken;
import fi.quanfoxes.Lexer.NumberToken;
import fi.quanfoxes.Parser.BackEnd.Intel.IntelProvider;
import fi.quanfoxes.Parser.instructions.AddInstruction;
import fi.quanfoxes.Parser.instructions.CreateLocalVariableInstruction;
import org.junit.Test;

import java.io.StringWriter;
import java.util.Arrays;

import static org.junit.Assert.assertSame;

public class IntelProviderTest {
    /*@Test
    public void addition_variableAndNumber () throws Exception {
        IntelProvider provider = new IntelProvider();
        StringWriter writer = new StringWriter();
        provider.setOutput(writer);

        CreateLocalVariableInstruction local = new CreateLocalVariableInstruction(DataTypes.get("num"), "a");
        AddInstruction addInstruction = new AddInstruction(new IdentifierToken("a"), new NumberToken(6));

        provider.Parameeter(Arrays.asList(local, addInstruction));

        assertSame("mov eax, [bp - 2]\nadd eax, 6", writer.toString());
    }*/
}