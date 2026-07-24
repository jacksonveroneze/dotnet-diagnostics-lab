namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;

/// <summary>
/// Validador de CPF decomposto em métodos pequenos.
/// A decomposição é intencional: cada etapa aparece isolada
/// no Back Traces do dotMemory, permitindo atribuir a alocação
/// ao passo exato do algoritmo.
/// </summary>
internal static class CpfValidator
{
    private const int CpfLength = 11;

    public static bool IsValid(
        string sourceCpf)
    {
        if (string.IsNullOrWhiteSpace(sourceCpf))
        {
            return false;
        }

        var clearCpf = Sanitize(sourceCpf);

        if (!HasValidLength(clearCpf))
        {
            return false;
        }

        if (IsRepeatedSequence(clearCpf))
        {
            return false;
        }

        if (!IsAllDigits(clearCpf))
        {
            return false;
        }

        var digits = ToDigitArray(clearCpf);

        var firstCheckDigit = CalculateFirstCheckDigit(digits);

        if (digits[9] != firstCheckDigit)
        {
            return false;
        }

        var secondCheckDigit = CalculateSecondCheckDigit(digits, firstCheckDigit);

        return digits[10] == secondCheckDigit;
    }

    /// <summary>
    /// ALOCA: até 3 strings (Trim + 2 Replace).
    /// Cada Replace cria uma nova string mesmo quando não há o que substituir.
    /// </summary>
    private static string Sanitize(
        string sourceCpf)
    {
        var clearCpf = sourceCpf.Trim();

        clearCpf = clearCpf.Replace("-", string.Empty);
        clearCpf = clearCpf.Replace(".", string.Empty);

        return clearCpf;
    }

    /// <summary>
    /// Não aloca.
    /// </summary>
    private static bool HasValidLength(
        string clearCpf)
    {
        return clearCpf.Length == CpfLength;
    }

    /// <summary>
    /// Não aloca: os literais são internados pelo compilador.
    /// </summary>
    private static bool IsRepeatedSequence(
        string clearCpf)
    {
        return clearCpf.Equals("00000000000")
               || clearCpf.Equals("11111111111")
               || clearCpf.Equals("22222222222")
               || clearCpf.Equals("33333333333")
               || clearCpf.Equals("44444444444")
               || clearCpf.Equals("55555555555")
               || clearCpf.Equals("66666666666")
               || clearCpf.Equals("77777777777")
               || clearCpf.Equals("88888888888")
               || clearCpf.Equals("99999999999");
    }

    /// <summary>
    /// Não aloca.
    /// </summary>
    private static bool IsAllDigits(
        string clearCpf)
    {
        foreach (var c in clearCpf)
        {
            if (!char.IsNumber(c))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// ALOCA: 1 array + 11 strings de um caractere.
    /// O padrão int.Parse(char.ToString()) é o maior custo do validador.
    /// </summary>
    private static int[] ToDigitArray(
        string clearCpf)
    {
        var digits = new int[CpfLength];

        for (var i = 0; i < clearCpf.Length; i++)
        {
            digits[i] = int.Parse(clearCpf[i].ToString());
        }

        return digits;
    }

    /// <summary>
    /// Não aloca.
    /// </summary>
    private static int CalculateFirstCheckDigit(
        int[] digits)
    {
        var total = 0;

        for (var position = 0; position < CpfLength - 2; position++)
        {
            total += digits[position] * (10 - position);
        }

        return NormalizeCheckDigit(total % CpfLength);
    }

    /// <summary>
    /// Não aloca.
    /// </summary>
    private static int CalculateSecondCheckDigit(
        int[] digits,
        int firstCheckDigit)
    {
        var total = 0;

        for (var position = 0; position < CpfLength - 2; position++)
        {
            total += digits[position] * (11 - position);
        }

        total += firstCheckDigit * 2;

        return NormalizeCheckDigit(total % CpfLength);
    }

    /// <summary>
    /// Não aloca.
    /// </summary>
    private static int NormalizeCheckDigit(
        int mod)
    {
        return mod < 2 ? 0 : CpfLength - mod;
    }
}
