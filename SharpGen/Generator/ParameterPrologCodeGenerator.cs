﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpGen.Model;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharpGen.Generator
{
    /// <summary>
    /// Declares local variables and allocates memory required for marshalling parameters to their native views.
    /// </summary>
    class ParameterPrologCodeGenerator : MarshallingCodeGeneratorBase, IMultiCodeGenerator<CsParameter, StatementSyntax>
    {
        public ParameterPrologCodeGenerator(GlobalNamespaceProvider globalNamespace) : base(globalNamespace)
        {
        }

        public IEnumerable<StatementSyntax> GenerateCode(CsParameter csElement)
        {
            if (csElement.IsUsedAsReturnType)
            {
                yield return LocalDeclarationStatement(
                    VariableDeclaration(
                        csElement.IsArray ? ArrayType(ParseTypeName(csElement.PublicType.QualifiedName), SingletonList(ArrayRankSpecifier())) : ParseTypeName(csElement.PublicType.QualifiedName),
                        SingletonSeparatedList(
                            VariableDeclarator(csElement.Name))));
            }
            
            if (csElement.HasNativeValueType)
            {
                if (csElement.IsArray)
                {
                    yield return LocalDeclarationStatement(
                        VariableDeclaration(
                            ArrayType(ParseTypeName($"{csElement.PublicType.QualifiedName}.__Native"), SingletonList(ArrayRankSpecifier())),
                            SingletonSeparatedList(
                                VariableDeclarator(GetMarshalStorageLocationIdentifier(csElement))
                                    .WithInitializer(EqualsValueClause(
                                        GenerateNullCheckIfNeeded(csElement,
                                            ObjectCreationExpression(
                                                ArrayType(ParseTypeName($"{csElement.PublicType.QualifiedName}.__Native"),
                                                SingletonList(ArrayRankSpecifier(
                                                    SingletonSeparatedList<ExpressionSyntax>(
                                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName(csElement.Name),
                                                            IdentifierName("Length"))))))),
                                            LiteralExpression(SyntaxKind.NullLiteralExpression)))))));
                }
                else
                {
                    yield return LocalDeclarationStatement(
                        VariableDeclaration(ParseTypeName($"{csElement.PublicType.QualifiedName}.__Native"),
                            SingletonSeparatedList(
                                VariableDeclarator(GetMarshalStorageLocationIdentifier(csElement))
                                .WithInitializer(EqualsValueClause(GetConstructorSyntax(csElement.PublicType as CsStruct))))));
                }
            }
            else if (csElement.IsNullableStruct)
            {
                yield return LocalDeclarationStatement(
                    VariableDeclaration(ParseTypeName(csElement.PublicType.QualifiedName),
                        SingletonSeparatedList(
                            VariableDeclarator(GetMarshalStorageLocationIdentifier(csElement)))));
                yield break;
            }

            if (csElement.IsInterface)
            {
                if (csElement.IsArray)
                {
                    yield return LocalDeclarationStatement(
                        VariableDeclaration(
                            PointerType(
                                QualifiedName(
                                    IdentifierName("System"),
                                    IdentifierName("IntPtr"))),
                            SingletonSeparatedList(
                                VariableDeclarator(GetMarshalStorageLocationIdentifier(csElement)))));
                    yield return ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(GetMarshalStorageLocationIdentifier(csElement)),
                            CastExpression(
                                PointerType(
                                    QualifiedName(
                                        IdentifierName("System"),
                                        IdentifierName("IntPtr"))),
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal(0)))));
                    yield return GenerateNullCheckIfNeeded(csElement,
                        Block(
                            LocalDeclarationStatement(
                                VariableDeclaration(
                                    PointerType(
                                        QualifiedName(
                                            IdentifierName("System"),
                                            IdentifierName("IntPtr"))),
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                            Identifier(csElement.IntermediateMarshalName))
                                        .WithInitializer(
                                            EqualsValueClause(
                                                StackAllocArrayCreationExpression(
                                                    ArrayType(
                                                        QualifiedName(
                                                            IdentifierName("System"),
                                                            IdentifierName("IntPtr")),
                                                        SingletonList(
                                                                ArrayRankSpecifier(
                                                                    SingletonSeparatedList<ExpressionSyntax>(
                                                                        MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            IdentifierName(csElement.Name),
                                                                            IdentifierName("Length")))))))))))),
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName(GetMarshalStorageLocationIdentifier(csElement)),
                                    IdentifierName(csElement.IntermediateMarshalName)))));
                }
                else
                {
                    yield return LocalDeclarationStatement(
                        VariableDeclaration(
                            QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("IntPtr")),
                            SingletonSeparatedList(
                                VariableDeclarator(GetMarshalStorageLocationIdentifier(csElement))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName("System"),
                                                    IdentifierName("IntPtr")),
                                                IdentifierName("Zero")))))));
                }
                yield break;
            }

            if (csElement.IsOut && !csElement.IsArray)
            {
                if (csElement.IsValueType && !csElement.IsPrimitive)
                {
                    yield return ExpressionStatement(
                        AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(csElement.Name),
                            ObjectCreationExpression(ParseTypeName(csElement.PublicType.QualifiedName))
                                .WithArgumentList(ArgumentList())
                    ));
                }
                else if (csElement.IsBoolToInt)
                {
                    yield return LocalDeclarationStatement(
                        VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)),
                            SingletonSeparatedList(VariableDeclarator(GetMarshalStorageLocationIdentifier(csElement)))));
                }
            }
            else if (csElement.IsString && !csElement.IsWideChar)
            {
                yield return LocalDeclarationStatement(
                    VariableDeclaration(
                        QualifiedName(
                            IdentifierName("System"),
                            IdentifierName("IntPtr")),
                        SingletonSeparatedList(
                            VariableDeclarator(GetMarshalStorageLocationIdentifier(csElement)))));
            }
        }
        
        private ExpressionSyntax GetConstructorSyntax(CsStruct structType)
        {
            if (structType.HasCustomNew)
            {
                return InvocationExpression(ParseExpression($"{structType.QualifiedName}.__NewNative"));
            }
            else
            {
                return ObjectCreationExpression(ParseTypeName($"{structType.QualifiedName}.__Native"))
                    .WithArgumentList(ArgumentList());
            }
        }
    }
}
