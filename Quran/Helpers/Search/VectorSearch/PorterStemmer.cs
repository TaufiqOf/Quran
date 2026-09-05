namespace Quran.Helpers.Search.VectorSearch;

/// <summary>
/// Porter Stemmer algorithm implementation for English stemming.
/// </summary>
public static class PorterStemmer
{
    public static string Stem(string word)
    {
        if (string.IsNullOrWhiteSpace(word) || word.Length <= 2)
            return word;

        var stemmer = new StemmerWorker(word.ToLower());
        return stemmer.Stem();
    }

    private class StemmerWorker(string word)
    {
        private readonly char[] _b = word.ToCharArray();
        private int _i = word.Length - 1;
        private int _i0 = 0;
        private int _j;
        private int _k = word.Length - 1;

        private bool Cons(int i)
        {
            switch (_b[i])
            {
                case 'a': case 'e': case 'i': case 'o': case 'u': return false;
                case 'y': return i == _i0 || !Cons(i - 1);
                default: return true;
            }
        }

        private int M()
        {
            int n = 0;
            int i = _i0;
            while (true)
            {
                if (i > _j) return n;
                if (!Cons(i)) break;
                i++;
            }
            i++;
            while (true)
            {
                while (true)
                {
                    if (i > _j) return n;
                    if (Cons(i)) break;
                    i++;
                }
                i++;
                n++;
                while (true)
                {
                    if (i > _j) return n;
                    if (!Cons(i)) break;
                    i++;
                }
                i++;
            }
        }

        private bool VowelInStem()
        {
            for (int i = _i0; i <= _j; i++)
            {
                if (!Cons(i)) return true;
            }
            return false;
        }

        private bool Doublec(int i)
        {
            if (i < _i0 + 1) return false;
            if (_b[i] != _b[i - 1]) return false;
            return Cons(i);
        }

        private bool Cvc(int i)
        {
            if (i < _i0 + 2 || !Cons(i) || Cons(i - 1) || !Cons(i - 2)) return false;
            int ch = _b[i];
            if (ch == 'w' || ch == 'x' || ch == 'y') return false;
            return true;
        }

        private bool Ends(string s)
        {
            int l = s.Length;
            int o = _k - l + 1;
            if (o < _i0) return false;
            for (int i = 0; i < l; i++)
            {
                if (_b[o + i] != s[i]) return false;
            }
            _j = _k - l;
            return true;
        }

        private void SetTo(string s)
        {
            int l = s.Length;
            int o = _j + 1;
            for (int i = 0; i < l; i++) _b[o + i] = s[i];
            _k = _j + l;
        }

        private void R(string s)
        {
            if (M() > 0) SetTo(s);
        }

        private void Step1()
        {
            if (_b[_k] == 's')
            {
                if (Ends("sses")) _k -= 2;
                else if (Ends("ies")) SetTo("i");
                else if (_b[_k - 1] != 's') _k--;
            }
            if (Ends("eed")) { if (M() > 0) _k--; }
            else if ((Ends("ed") || Ends("ing")) && VowelInStem())
            {
                _k = _j;
                if (Ends("at")) SetTo("ate");
                else if (Ends("bl")) SetTo("ble");
                else if (Ends("iz")) SetTo("ize");
                else if (Doublec(_k))
                {
                    _k--;
                    int ch = _b[_k];
                    if (ch == 'l' || ch == 's' || ch == 'z') _k++;
                }
                else if (M() == 1 && Cvc(_k)) SetTo("e");
            }
        }

        private void Step2() { if (Ends("y") && VowelInStem()) _b[_k] = 'i'; }

        private void Step3()
        {
            if (_k == _i0) return;
            switch (_b[_k - 1])
            {
                case 'a':
                    if (Ends("ational")) { R("ate"); break; }
                    if (Ends("tional")) { R("tion"); break; }
                    break;
                case 'c':
                    if (Ends("enci")) { R("ence"); break; }
                    if (Ends("anci")) { R("ance"); break; }
                    break;
                case 'e': if (Ends("izer")) { R("ize"); break; } break;
                case 'l':
                    if (Ends("bli")) { R("ble"); break; }
                    if (Ends("alli")) { R("al"); break; }
                    if (Ends("entli")) { R("ent"); break; }
                    if (Ends("eli")) { R(""); break; }
                    if (Ends("ousli")) { R("ous"); break; }
                    break;
                case 'o':
                    if (Ends("ization")) { R("ize"); break; }
                    if (Ends("ation")) { R("ate"); break; }
                    if (Ends("ator")) { R("ate"); break; }
                    break;
                case 's':
                    if (Ends("alism")) { R("al"); break; }
                    if (Ends("iveness")) { R("ive"); break; }
                    if (Ends("fulness")) { R("ful"); break; }
                    if (Ends("ousness")) { R("ous"); break; }
                    break;
                case 't':
                    if (Ends("aliti")) { R("al"); break; }
                    if (Ends("iviti")) { R("ive"); break; }
                    if (Ends("biliti")) { R("ble"); break; }
                    break;
                case 'g': if (Ends("logi")) { R("log"); break; } break;
            }
        }

        private void Step4()
        {
            switch (_b[_k])
            {
                case 'e':
                    if (Ends("icate")) { R("ic"); break; }
                    if (Ends("ative")) { R(""); break; }
                    if (Ends("alize")) { R("al"); break; }
                    break;
                case 'i': if (Ends("iciti")) { R("ic"); break; } break;
                case 'l':
                    if (Ends("ical")) { R("ic"); break; }
                    if (Ends("ful")) { R(""); break; }
                    break;
                case 's': if (Ends("ness")) { R(""); break; } break;
            }
        }

        private void Step5()
        {
            if (_k == _i0) return;
            switch (_b[_k - 1])
            {
                case 'a': if (Ends("al")) break; return;
                case 'c': if (Ends("ance") || Ends("ence")) break; return;
                case 'e': if (Ends("er")) break; return;
                case 'i': if (Ends("ic")) break; return;
                case 'l': if (Ends("able") || Ends("ible")) break; return;
                case 'n': if (Ends("ant") || Ends("ement") || Ends("ment") || Ends("ent")) break; return;
                case 'o': if (Ends("ion") && _j >= _i0 && (_b[_j] == 's' || _b[_j] == 't')) break;
                    if (Ends("ou")) break; return;
                case 's': if (Ends("ism")) break; return;
                case 't': if (Ends("ate") || Ends("iti")) break; return;
                case 'u': if (Ends("ous")) break; return;
                case 'v': if (Ends("ive")) break; return;
                case 'z': if (Ends("ize")) break; return;
                default: return;
            }
            if (M() > 1) _k = _j;
        }

        private void Step6()
        {
            _j = _k;
            if (_b[_k] == 'e')
            {
                int a = M();
                if (a > 1 || (a == 1 && !Cvc(_k - 1))) _k--;
            }
            if (_b[_k] == 'l' && Doublec(_k) && M() > 1) _k--;
        }

        public string Stem()
        {
            Step1();
            Step2();
            Step3();
            Step4();
            Step5();
            Step6();
            return new string(_b, 0, _k + 1);
        }
    }
}