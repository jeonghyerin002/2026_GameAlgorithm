using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace AlgoLesson6
{
    public sealed class AlgorithmPokerGame : MonoBehaviour
    {
        private const int HandSize = 8;
        private readonly PokerDeck deck = new PokerDeck();
        private readonly List<CardData> hand = new List<CardData>();
        private readonly HashSet<int> selected = new HashSet<int>();
        private IPokerHandEvaluator evaluator;
        private BestHandAdvisor advisor;
        private int score;
        private int targetScore = 350;
        private int handsLeft;
        private int discardsLeft;
        private int round = 1;
        private int seed;
        private string message = "카드를 선택하고 PLAY HAND를 누르세요.";
        private PokerHandResult lastResult;
        private bool isAnimating;
        private float playProgress;
        private float dealStartedAt;
        private float displayedScore;
        private float flashAmount;
        private float scorePulse;
        private float roundBannerUntil;
        private string comboBanner = "";
        private List<int> playingIndices = new List<int>();
        private GUIStyle cardStyle;
        private GUIStyle selectedStyle;
        private GUIStyle titleStyle;
        private GUIStyle centerStyle;
        private GUIStyle comboStyle;
        private GUIStyle cardCornerStyle;
        private GUIStyle cardSuitStyle;
        private GUIStyle cardBottomStyle;
        private Texture2D whiteCardTexture;
        private Texture2D redCardTexture;
        private Texture2D selectedCardTexture;
        private Texture2D shadowTexture;
        private Texture2D flashTexture;

        private void Awake()
        {
            evaluator = new PokerHandEvaluator();
            advisor = new BestHandAdvisor(evaluator);
            StartRun();
        }

        private void Update()
        {
            displayedScore = Mathf.MoveTowards(displayedScore, score, Mathf.Max(150f, Mathf.Abs(score - displayedScore) * 5f) * Time.unscaledDeltaTime);
            flashAmount = Mathf.MoveTowards(flashAmount, 0f, 2.8f * Time.unscaledDeltaTime);
            scorePulse = Mathf.MoveTowards(scorePulse, 0f, 2.2f * Time.unscaledDeltaTime);

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.rKey.wasPressedThisFrame) { StopAllCoroutines(); StartRun(); return; }
            if (isAnimating) return;
            if (keyboard.enterKey.wasPressedThisFrame) PlaySelected();
            if (keyboard.backspaceKey.wasPressedThisFrame) DiscardSelected();
            if (keyboard.hKey.wasPressedThisFrame) ShowBestHint();
            KeyControl[] keys = { keyboard.digit1Key, keyboard.digit2Key, keyboard.digit3Key, keyboard.digit4Key,
                keyboard.digit5Key, keyboard.digit6Key, keyboard.digit7Key, keyboard.digit8Key };
            for (int i = 0; i < keys.Length && i < hand.Count; i++) if (keys[i].wasPressedThisFrame) ToggleCard(i);
        }

        private void StartRun()
        {
            seed = Environment.TickCount;
            deck.ResetAndShuffle(seed);
            hand.Clear(); selected.Clear(); playingIndices.Clear();
            score = 0; displayedScore = 0; round = 1; handsLeft = 4; discardsLeft = 3;
            isAnimating = false; playProgress = 0; flashAmount = 0; scorePulse = 0;
            lastResult = null; DrawToHand();
            roundBannerUntil = Time.unscaledTime + 1.4f;
            message = "최대 5장을 선택하세요. 숫자키 1~8 또는 카드 클릭";
        }

        private void ToggleCard(int index)
        {
            if (index < 0 || index >= hand.Count) return;
            if (selected.Contains(index)) selected.Remove(index);
            else if (selected.Count < 5) selected.Add(index);
            else message = "한 번에 최대 5장만 선택할 수 있습니다.";
        }

        private List<CardData> SelectedCards()
        {
            List<int> indices = new List<int>(selected); indices.Sort();
            List<CardData> result = new List<CardData>();
            foreach (int index in indices) if (index >= 0 && index < hand.Count) result.Add(hand[index]);
            return result;
        }

        private void PlaySelected()
        {
            if (isAnimating) return;
            if (selected.Count == 0) { message = "낼 카드를 선택하세요."; return; }
            if (handsLeft <= 0) { message = "남은 Hand가 없습니다. R로 다시 시작하세요."; return; }
            StartCoroutine(PlayHandSequence());
        }

        private System.Collections.IEnumerator PlayHandSequence()
        {
            isAnimating = true;
            playingIndices = new List<int>(selected);
            playingIndices.Sort();
            lastResult = evaluator.Evaluate(SelectedCards());
            comboBanner = lastResult.Name;
            message = "패를 공개합니다...";

            float elapsed = 0f;
            const float gatherDuration = .55f;
            while (elapsed < gatherDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                playProgress = Mathf.SmoothStep(0f, 1f, elapsed / gatherDuration);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(.18f);
            score += lastResult.Score;
            handsLeft--;
            flashAmount = .72f;
            scorePulse = 1f;
            message = $"{lastResult.Name}: ({lastResult.baseChips}+{lastResult.cardChips}) × {lastResult.multiplier} = {lastResult.Score:N0}";
            yield return new WaitForSecondsRealtime(.9f);

            RemoveSelectedAndDraw();
            playingIndices.Clear();
            playProgress = 0f;
            isAnimating = false;

            if (score >= targetScore)
            {
                message = "목표 달성! 다음 라운드로 진행합니다.";
                yield return new WaitForSecondsRealtime(.35f);
                NextRound();
            }
            else if (handsLeft == 0)
            {
                message = "라운드 실패! R로 다시 시작하세요.";
            }
        }

        private void DiscardSelected()
        {
            if (isAnimating) return;
            if (selected.Count == 0) { message = "버릴 카드를 선택하세요."; return; }
            if (discardsLeft <= 0) { message = "남은 Discard가 없습니다."; return; }
            discardsLeft--; RemoveSelectedAndDraw(); message = "선택한 카드를 버리고 새 카드를 받았습니다.";
        }

        private void RemoveSelectedAndDraw()
        {
            List<int> indices = new List<int>(selected); indices.Sort((a, b) => b.CompareTo(a));
            foreach (int index in indices) if (index >= 0 && index < hand.Count) hand.RemoveAt(index);
            selected.Clear(); DrawToHand();
        }

        private void DrawToHand()
        {
            while (hand.Count < HandSize && deck.TryDraw(out CardData card)) hand.Add(card);
            dealStartedAt = Time.unscaledTime;
        }

        private void NextRound()
        {
            round++; targetScore *= 2; handsLeft = 4; discardsLeft = 3;
            deck.ResetAndShuffle(seed + round * 997); hand.Clear(); selected.Clear(); DrawToHand();
            roundBannerUntil = Time.unscaledTime + 1.6f;
            flashAmount = .35f;
        }

        private void ShowBestHint()
        {
            PokerHandResult best = advisor.FindBest(hand);
            message = best.cards.Count == 0 ? "힌트를 계산하지 못했습니다." : $"HINT: {best.Name}, 예상 점수 {best.Score} / {string.Join(" ", best.cards)}";
        }

        private void EnsureStyles()
        {
            if (cardStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            titleStyle.normal.textColor = new Color(.95f, .85f, .36f);
            centerStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            centerStyle.normal.textColor = Color.white;
            cardStyle = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            whiteCardTexture = MakeTexture(new Color(.94f, .94f, .89f));
            redCardTexture = MakeTexture(new Color(1f, .88f, .88f));
            selectedCardTexture = MakeTexture(new Color(.98f, .72f, .16f));
            shadowTexture = MakeTexture(new Color(0f, 0f, 0f, .48f));
            flashTexture = MakeTexture(new Color(1f, .78f, .18f, 1f));
            cardStyle.normal.textColor = new Color(.06f, .07f, .09f);
            selectedStyle = new GUIStyle(cardStyle);
            selectedStyle.normal.textColor = new Color(.06f, .07f, .09f);
            comboStyle = new GUIStyle(titleStyle) { fontSize = 42 };
            cardCornerStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperLeft };
            cardSuitStyle = new GUIStyle(GUI.skin.label) { fontSize = 54, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            cardBottomStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.LowerRight };
            comboStyle.normal.textColor = new Color(1f, .82f, .2f);
        }

        private Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1); texture.SetPixel(0, 0, color); texture.Apply(); return texture;
        }

        private void OnGUI()
        {
            EnsureStyles();
            float width = Mathf.Min(Screen.width - 40, 1120);
            float shake = scorePulse > 0f ? Mathf.Sin(Time.unscaledTime * 85f) * scorePulse * 7f : 0f;
            float left = (Screen.width - width) * .5f + shake;
            GUI.color = new Color(.035f, .09f, .08f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteCardTexture);
            GUI.color = Color.white;
            GUI.Box(new Rect(left, 18, width, Screen.height - 36), "");
            GUI.Label(new Rect(left + 20, 28, width - 40, 45), "ALGORITHM POKER", titleStyle);

            int shownScore = Mathf.RoundToInt(displayedScore);
            float pulse = 1f + scorePulse * .18f;
            GUIUtility.ScaleAroundPivot(new Vector2(pulse, pulse), new Vector2(left + width * .5f, 104));
            GUI.Label(new Rect(left + 25, 77, width - 50, 60), $"ROUND {round}     SCORE {shownScore:N0} / {targetScore:N0}     HANDS {handsLeft}     DISCARDS {discardsLeft}     DECK {deck.Remaining}", centerStyle);
            GUI.matrix = Matrix4x4.identity;

            if (isAnimating && lastResult != null)
            {
                float bannerScale = 1f + Mathf.Sin(Time.unscaledTime * 9f) * .04f;
                GUIUtility.ScaleAroundPivot(new Vector2(bannerScale, bannerScale), new Vector2(Screen.width * .5f, 190));
                GUI.Label(new Rect(left + 50, 112, width - 100, 58), comboBanner, comboStyle);
                GUI.Label(new Rect(left + 50, Screen.height * .20f + 182f, width - 100, 38), $"{lastResult.baseChips} + {lastResult.cardChips} CHIPS     × {lastResult.multiplier} MULT", centerStyle);
                GUI.matrix = Matrix4x4.identity;
            }

            float gap = 10f;
            float cardWidth = (width - 70 - gap * 7) / 8f;
            float cardHeight = 170f;
            float cardY = Screen.height * .53f;
            Vector2 mouse = Event.current.mousePosition;
            for (int i = 0; i < hand.Count; i++)
            {
                bool picked = selected.Contains(i);
                float finalX = left + 35 + i * (cardWidth + gap);
                float finalY = picked ? cardY - 58 : cardY;
                float dealT = Mathf.Clamp01((Time.unscaledTime - dealStartedAt - i * .075f) / .42f);
                dealT = 1f - Mathf.Pow(1f - dealT, 3f);
                float x = Mathf.Lerp(left + width - 70, finalX, dealT);
                float y = Mathf.Lerp(115f, finalY, dealT);

                int playOrder = playingIndices.IndexOf(i);
                if (playOrder >= 0)
                {
                    float stagger = playOrder * .055f;
                    float localProgress = Mathf.Clamp01((playProgress - stagger) / Mathf.Max(.01f, 1f - stagger));
                    float groupWidth = playingIndices.Count * (cardWidth * .72f);
                    float targetX = left + width * .5f - groupWidth * .5f + playOrder * cardWidth * .72f;
                    float targetY = Screen.height * .20f;
                    float arc = Mathf.Sin(localProgress * Mathf.PI) * 72f;
                    x = Mathf.Lerp(finalX, targetX, localProgress);
                    y = Mathf.Lerp(finalY, targetY, localProgress) - arc;
                }

                Rect rect = new Rect(x, y, cardWidth, cardHeight);
                bool hover = !isAnimating && rect.Contains(mouse);
                if (hover)
                {
                    rect.x -= 4f;
                    rect.y -= 16f;
                    rect.width += 8f;
                    rect.height += 10f;
                }

                float fanCenter = (hand.Count - 1) * .5f;
                float angle = isAnimating ? 0f : (i - fanCenter) * 1.8f;
                Matrix4x4 previousMatrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(angle, rect.center);
                GUI.DrawTexture(new Rect(rect.x + 8, rect.y + 10, rect.width, rect.height), shadowTexture);

                CardData card = hand[i];
                bool redSuit = card.suit == CardSuit.Hearts || card.suit == CardSuit.Diamonds;
                Texture2D face = redSuit ? redCardTexture : whiteCardTexture;
                if (picked) GUI.DrawTexture(new Rect(rect.x - 5, rect.y - 5, rect.width + 10, rect.height + 10), selectedCardTexture);
                GUI.DrawTexture(rect, face);

                Color textColor = redSuit ? new Color(.78f, .06f, .1f) : new Color(.025f, .03f, .045f);
                cardCornerStyle.normal.textColor = textColor;
                cardSuitStyle.normal.textColor = textColor;
                cardBottomStyle.normal.textColor = textColor;
                Rect inset = new Rect(rect.x + 10f, rect.y + 7f, rect.width - 20f, rect.height - 14f);
                GUI.Label(inset, card.RankText + "\n" + card.SuitText, cardCornerStyle);
                GUI.Label(rect, card.SuitText, cardSuitStyle);
                GUI.Label(inset, card.SuitText + "  " + card.RankText, cardBottomStyle);
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none) && !isAnimating) ToggleCard(i);

                if (playOrder >= 0 && playProgress > .78f)
                {
                    float popup = Mathf.Clamp01((playProgress - .78f) / .22f);
                    Color oldColor = GUI.color;
                    GUI.color = new Color(1f, .88f, .2f, popup);
                    GUI.Label(new Rect(rect.x, rect.y - 34f - popup * 12f, rect.width, 30f), "+" + card.ChipValue, centerStyle);
                    GUI.color = oldColor;
                }
                GUI.matrix = previousMatrix;
            }

            float buttonY = cardY + cardHeight + 42;
            GUI.enabled = !isAnimating;
            if (GUI.Button(new Rect(left + width * .2f - 90, buttonY, 180, 44), "PLAY HAND [Enter]")) PlaySelected();
            if (GUI.Button(new Rect(left + width * .5f - 90, buttonY, 180, 44), "DISCARD [Backspace]")) DiscardSelected();
            if (GUI.Button(new Rect(left + width * .8f - 90, buttonY, 180, 44), "BEST HINT [H]")) ShowBestHint();
            GUI.enabled = true;
            GUI.Label(new Rect(left + 35, buttonY + 58, width - 70, 78), message, centerStyle);
            GUI.Label(new Rect(left + 35, Screen.height - 80, width - 70, 30), "R: 새 게임  |  1~8: 카드 선택  |  최대 5장", centerStyle);

            if (Time.unscaledTime < roundBannerUntil)
            {
                float alpha = Mathf.Clamp01((roundBannerUntil - Time.unscaledTime) * 1.5f);
                Color old = GUI.color; GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.Label(new Rect(0, Screen.height * .42f, Screen.width, 80), "ROUND " + round, comboStyle);
                GUI.color = old;
            }
            if (flashAmount > 0f)
            {
                Color old = GUI.color; GUI.color = new Color(1f, 1f, 1f, flashAmount * .32f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), flashTexture);
                GUI.color = old;
            }
        }
    }
}





