using InputEnum;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemTests : InputTestFixture
{
    private Keyboard keyboard;
    private Mouse mouse;
    private InputManager manager;

    private bool attackCalled;
    private bool inventoryCalled;
    private Vector2 moveValue;

    public override void Setup()
    {
        base.Setup();

        keyboard = InputSystem.AddDevice<Keyboard>();
        mouse = InputSystem.AddDevice<Mouse>();

        InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/20_InputActions/InputActions.inputactions");
        Assert.IsNotNull(asset);

        var go = new GameObject("InputManager");
        manager = go.AddComponent<InputManager>();
        manager.Initialize(asset);

        BindActions();
    }

    private void BindActions()
    {
        // Bind
        var attack = manager.Maps[ActionMaps.Gameplay].FindAction(Actions.Attack.ToString());
        attack.performed += ctx =>
        {
            attackCalled = true;
            Debug.Log("[LOG] Attack triggered");
        };

        var move = manager.Maps[ActionMaps.Gameplay].FindAction(Actions.Move.ToString());
        move.performed += ctx =>
        {
            moveValue = ctx.ReadValue<Vector2>();
            Debug.Log($"[LOG] Move value: {moveValue}");
        };

        var inventory = manager.Maps[ActionMaps.UI].FindAction(Actions.Inventory.ToString());
        inventory.performed += _ =>
        {
            inventoryCalled = true;
        };
    }

    public override void TearDown()
    {
        Object.DestroyImmediate(manager.gameObject);
        base.TearDown();
    }

    // -----------------------------
    // 1. 좌클릭 바인딩 확인
    // -----------------------------
    [Test]
    public void _1_바인딩_확인_GamePlay_Attack_좌클릭()
    {
        attackCalled = false;

        manager.SetLayer(ActionMaps.Gameplay);

        Press(mouse.leftButton);
        InputSystem.Update();

        Assert.IsTrue(attackCalled, "좌클릭 바인딩이 동작하지 않음");
    }

    // -----------------------------
    // 2. 이동 입력 확인
    // -----------------------------
    [Test]
    public void _2_입력_확인_GamePlay_Move()
    {
        moveValue = Vector2.zero;

        manager.SetLayer(ActionMaps.Gameplay);

        Press(keyboard.wKey);
        InputSystem.Update();

        Assert.AreEqual(Vector2.up, moveValue);
    }

    // -----------------------------
    // 3. Asset 연결 확인
    // -----------------------------
    [Test]
    public void _3_전체_InputAsset_연결_확인()
    {
        Assert.IsNotNull(manager.Maps);
        foreach (ActionMaps maps in System.Enum.GetValues(typeof(ActionMaps)))
        {
            if (maps == ActionMaps.None) continue;
            Assert.IsTrue(manager.Maps.ContainsKey(maps), $"ActionMap '{maps}'가 매니저에 없음");
        }
    }

    // -----------------------------
    // 4. Layer Lock 확인
    // -----------------------------
    [Test]
    public void _4_레이어_전체_입력_잠금()
    {
        attackCalled = false;

        manager.SetLayer(ActionMaps.None);

        Press(mouse.leftButton);
        InputSystem.Update();

        Assert.IsFalse(attackCalled);
    }

    // -----------------------------
    // 5. AddLayer 확인
    // -----------------------------
    [Test]
    public void _5_레이어_추가_Gameplay()
    {
        attackCalled = false;

        manager.SetLayer(ActionMaps.None);
        manager.AddLayer(ActionMaps.Gameplay);

        Press(mouse.leftButton);
        InputSystem.Update();

        Assert.IsTrue(attackCalled);
    }

    // -----------------------------
    // 6. RemoveLayer 확인
    // -----------------------------
    [Test]
    public void _6_레이어_제거_Gameplay()
    {
        attackCalled = false;

        manager.SetLayer(ActionMaps.Gameplay);
        manager.RemoveLayer(ActionMaps.Gameplay);

        Press(mouse.leftButton);
        InputSystem.Update();

        Assert.IsFalse(attackCalled);
    }

    // -----------------------------
    // 7. 리바인딩 테스트
    // 좌클릭 → F 키로 변경
    // -----------------------------
    [Test]
    public void _7_입력_리바인드_Gameplay_Attack()
    {
        attackCalled = false;

        manager.SetLayer(ActionMaps.Gameplay);

        var attack = manager.Maps[ActionMaps.Gameplay]
            .FindAction(Actions.Attack.ToString());

        // 좌클릭 제거하고 F로 변경
        attack.ApplyBindingOverride("<Keyboard>/f");

        // 기존 좌클릭은 동작하면 안됨
        Press(mouse.leftButton);
        InputSystem.Update();

        Assert.IsFalse(attackCalled);

        // 새 바인딩 F는 동작해야 함
        Press(keyboard.fKey);
        InputSystem.Update();

        Assert.IsTrue(attackCalled);
    }
}