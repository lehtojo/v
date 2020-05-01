using System;

public static class ArithmeticOperators
{
    public static Result Build(Unit unit, IncrementNode node)
    {
        return BuildIncrementOperation(unit, node);
    }

    public static Result Build(Unit unit, OperatorNode node)
    {
        /// TODO: Create a register preference system dependent on the situation
        var operation = node.Operator;
        
        if (operation == Operators.ADD)
        {
            return BuildAdditionOperator(unit, node);
        }
        else if (operation == Operators.SUBTRACT)
        {
            return BuildSubtractionOperator(unit, node);
        }
        else if (operation == Operators.MULTIPLY)
        {
            return BuildMultiplicationOperator(unit, node);
        }
        else if (operation == Operators.DIVIDE)
        {
            return BuildDivisionOperator(unit, false, node);
        }
        else if (operation == Operators.MODULUS)
        {
            return BuildDivisionOperator(unit, true, node);
        }
        else if (operation == Operators.ASSIGN_ADD)
        {
            return BuildAdditionOperator(unit, node, true);
        }
        else if (operation == Operators.ASSIGN_SUBTRACT)
        {
            return BuildSubtractionOperator(unit, node, true);
        }
        else if (operation == Operators.ASSIGN_MULTIPLY)
        {
            return BuildMultiplicationOperator(unit, node, true);
        }
        else if (operation == Operators.ASSIGN_DIVIDE)
        {
            return BuildDivisionOperator(unit, false, node, true);
        }
        else if (operation == Operators.ASSIGN_MODULUS)
        {
            return BuildDivisionOperator(unit, true, node, true);
        }
        else if (operation == Operators.ASSIGN)
        {
            return BuildAssignOperator(unit, node);
        }
        else if (operation == Operators.COLON)
        {
            return Arrays.BuildOffset(unit, node, AccessMode.READ);
        }

        throw new ArgumentException("Node not implemented yet");
    }

    public static Result BuildNegate(Unit unit, NegateNode node)
    {
        return new NegateInstruction(unit, References.Get(unit, node.Target)).Execute();
    }

    public static Result BuildAdditionOperator(Unit unit, OperatorNode operation, bool assigns = false)
    {   
        var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
        var right = References.Get(unit, operation.Right);

        var number_type = operation.GetType()!.To<Number>().Type;

        return new AdditionInstruction(unit, left, right, number_type, assigns).Execute();
    }

    public static Result BuildIncrementOperation(Unit unit, IncrementNode increment)
    {   
        var left = References.Get(unit, increment.Object, AccessMode.WRITE);
        var right = References.Get(unit, new NumberNode(Assembler.Size.ToFormat(false), 1));

        var number_type = ((IType)increment.Object).GetType()!.To<Number>().Type;

        return new AdditionInstruction(unit, left, right, number_type, true).Execute();
    }

    public static Result BuildSubtractionOperator(Unit unit, OperatorNode operation, bool assigns = false)
    {
        var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
        var right = References.Get(unit, operation.Right);
        
        var number_type = operation.GetType()!.To<Number>().Type;

        return new SubtractionInstruction(unit, left, right, number_type, assigns).Execute();
    }

    public static Result BuildMultiplicationOperator(Unit unit, OperatorNode operation, bool assigns = false)
    {
        var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
        var right = References.Get(unit, operation.Right);
        
        var number_type = operation.GetType()!.To<Number>().Type;

        return new MultiplicationInstruction(unit, left, right, number_type, assigns).Execute();
    }

    public static Result BuildDivisionOperator(Unit unit, bool modulus, OperatorNode operation, bool assigns = false)
    {
        var left = References.Get(unit, operation.Left, assigns ? AccessMode.WRITE : AccessMode.READ);
        var right = References.Get(unit, operation.Right);
        
        var number_type = operation.GetType()!.To<Number>().Type;

        return new DivisionInstruction(unit, modulus, left, right, number_type, assigns).Execute();
    }

    public static Result BuildAssignOperator(Unit unit, OperatorNode node) 
    {
        var left = References.Get(unit, node.Left, AccessMode.WRITE);
        var right = References.Get(unit, node.Right);

        return new AssignInstruction(unit, left, right).Execute();
    }

    public static void GetDivisionConstants(int divider, int bits)
    {
        
    }
}