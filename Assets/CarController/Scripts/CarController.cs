/*
MENSAGEM DO CRIADOR: Este script foi codificado por Mena. Você pode usá‑lo nos seus jogos, sejam comerciais ou
projetos pessoais. Você pode até adicionar ou remover funções como desejar. No entanto, você não pode vender cópias
desse script isoladamente, pois ele é originalmente distribuído como um produto gratuito.
Desejo o melhor para o seu projeto. Boa sorte!

P.S.: Se você precisa de mais carros, pode conferir meus outros assets de veículos na Unity Asset Store; talvez
você encontre algo útil para o seu jogo. Atenciosamente, Mena.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class CarController : MonoBehaviour
{

    //CAR SETUP

      [Space(20)]
      //[Header("CAR SETUP")]
      [Space(10)]
      [Range(20, 190)]
      public int maxSpeed = 90; //Velocidade máxima que o carro pode atingir em km/h.
      [Range(10, 120)]
      public int maxReverseSpeed = 45; //Velocidade máxima que o carro pode atingir em marcha à ré em km/h.
      [Range(1, 10)]
      public int accelerationMultiplier = 2; // Quão rápido o carro acelera. 1 é lento e 10 é o mais rápido.
      [Space(10)]
      [Range(10, 45)]
      public int maxSteeringAngle = 27; // Ângulo máximo que os pneus podem alcançar ao girar o volante.
      [Range(0.1f, 1f)]
      public float steeringSpeed = 0.5f; // Velocidade com que o volante gira.
      [Space(10)]
      [Range(100, 600)]
      public int brakeForce = 350; // Força dos freios das rodas.
      [Range(1, 10)]
      public int decelerationMultiplier = 2; // Quão rápido o carro desacelera quando não há aceleração.
      [Range(1, 10)]
      public int handbrakeDriftMultiplier = 5; // Quanto de aderência o carro perde ao usar o freio de mão.
      [Space(10)]
      public Vector3 bodyMassCenter; // Vetor que contém o centro de massa do carro. Recomendo definir este valor
                                    // nos pontos x = 0 e z = 0 do seu carro. Você pode escolher o valor desejado no eixo y,
                                    // porém, quanto maior este valor, mais instável o carro se torna.
                                    // Normalmente o valor de y vai de 0 a 1.5.

    //WHEELS

      //[Header("WHEELS")]

      /*
      As variáveis a seguir armazenam os dados das rodas do carro. Precisamos dos objetos apenas de malha (mesh)
      e dos componentes WheelCollider das rodas. Os WheelColliders e as malhas 3D das rodas não podem vir do mesmo
      GameObject; eles devem ser GameObjects separados.
      */
      public GameObject frontLeftMesh;
      public WheelCollider frontLeftCollider;
      [Space(10)]
      public GameObject frontRightMesh;
      public WheelCollider frontRightCollider;
      [Space(10)]
      public GameObject rearLeftMesh;
      public WheelCollider rearLeftCollider;
      [Space(10)]
      public GameObject rearRightMesh;
      public WheelCollider rearRightCollider;

    //PARTICLE SYSTEMS

      [Space(20)]
      //[Header("EFFECTS")]
      [Space(10)]
      //A variável abaixo permite configurar sistemas de partículas no carro
      public bool useEffects = false;

      // Os sistemas de partículas abaixo são usados como fumaça do pneu quando o carro derrapa.
      public ParticleSystem RLWParticleSystem;
      public ParticleSystem RRWParticleSystem;

      [Space(10)]
      // Os trail renderers abaixo simulam marcas de pneus quando o carro perde tração.
      public TrailRenderer RLWTireSkid;
      public TrailRenderer RRWTireSkid;

    //SPEED TEXT (UI)

      [Space(20)]
      //[Header("UI")]
      [Space(10)]
      //A variável abaixo permite configurar um texto de UI para exibir a velocidade do carro.
      public bool useUI = false;
      public Text carSpeedText; // Used to store the UI object that is going to show the speed of the car.

    //SOUNDS

      [Space(20)]
      //[Header("Sounds")]
      [Space(10)]
      //A variável abaixo permite configurar sons do carro como motor e chiado dos pneus.
      public bool useSounds = false;
      public AudioSource carEngineSound; // Armazena o som do motor do carro.
      public AudioSource tireScreechSound; // Armazena o som do chiado dos pneus (quando o carro derrapa).
      float initialCarEngineSoundPitch; // Armazena o pitch inicial do som do motor.

    //CONTROLS

      [Space(20)]
      //[Header("CONTROLS")]
      [Space(10)]
      public InputActionReference move;
      public InputActionReference handbrakeAction;
      public InputActionReference accelerateAction;
      public InputActionReference reverseAction;
      [Range(0f,1f)] public float joystickSteerDeadzone = 0.15f;
      [Range(0f,1f)] public float accelerateButtonThreshold = 0.5f;
      [Range(0f,1f)] public float reverseButtonThreshold = 0.5f;
      [Range(0f,1f)] public float handbrakeButtonThreshold = 0.5f;
      public bool decelerateOnNoThrottle = true;

    //CAR DATA

      [HideInInspector]
      public float carSpeed; // Used to store the speed of the car.
      [HideInInspector]
      public bool isDrifting; // Used to know whether the car is drifting or not.
      [HideInInspector]
      public bool isTractionLocked; // Used to know whether the traction of the car is locked or not.

    //PRIVATE VARIABLES

      /*
      IMPORTANT: The following variables should not be modified manually since their values are automatically given via script.
      */
      Rigidbody carRigidbody; // Stores the car's rigidbody.
      float steeringAxis; // Used to know whether the steering wheel has reached the maximum value. It goes from -1 to 1.
      float throttleAxis; // Used to know whether the throttle has reached the maximum value. It goes from -1 to 1.
      float driftingAxis;
      float localVelocityZ;
      float localVelocityX;
      bool deceleratingCar;
      /*
      The following variables are used to store information about sideways friction of the wheels (such as
      extremumSlip,extremumValue, asymptoteSlip, asymptoteValue and stiffness). We change this values to
      make the car to start drifting.
      */
      WheelFrictionCurve FLwheelFriction;
      float FLWextremumSlip;
      WheelFrictionCurve FRwheelFriction;
      float FRWextremumSlip;
      WheelFrictionCurve RLwheelFriction;
      float RLWextremumSlip;
      WheelFrictionCurve RRwheelFriction;
      float RRWextremumSlip;

    // Start is called before the first frame update
    void Start()
    {
      //Nesta parte, definimos 'carRigidbody' com o Rigidbody anexado a este GameObject.
      //Também definimos o centro de massa do carro com o Vector3 fornecido no inspetor.
      carRigidbody = gameObject.GetComponent<Rigidbody>();
      carRigidbody.centerOfMass = bodyMassCenter;

      //Configuração inicial para calcular o valor de derrapagem do carro. Esta parte pode parecer
      //complicada, mas estamos apenas salvando os valores padrão de fricção das rodas para ajustar
      //um valor apropriado de derrapagem depois.
      FLwheelFriction = new WheelFrictionCurve ();
        FLwheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;
      FRwheelFriction = new WheelFrictionCurve ();
        FRwheelFriction.extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRWextremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = frontRightCollider.sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = frontRightCollider.sidewaysFriction.stiffness;
      RLwheelFriction = new WheelFrictionCurve ();
        RLwheelFriction.extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLWextremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = rearLeftCollider.sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = rearLeftCollider.sidewaysFriction.stiffness;
      RRwheelFriction = new WheelFrictionCurve ();
        RRwheelFriction.extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRWextremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = rearRightCollider.sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = rearRightCollider.sidewaysFriction.stiffness;

        // Salvamos o pitch inicial do som do motor do carro.
        if(carEngineSound != null){
          initialCarEngineSoundPitch = carEngineSound.pitch;
        }

        // Invocamos 2 métodos neste script. CarSpeedUI() atualiza o texto de UI que mostra a velocidade do carro
        // e CarSounds() controla os sons do motor e da derrapagem. Ambos são invocados em 0 segundos e repetidos
        // a cada 0.1 segundos.
        if(useUI){
          InvokeRepeating("CarSpeedUI", 0f, 0.1f);
        }else if(!useUI){
          if(carSpeedText != null){
            carSpeedText.text = "0";
          }
        }

        if(useSounds){
          InvokeRepeating("CarSounds", 0f, 0.1f);
        }else if(!useSounds){
          if(carEngineSound != null){
            carEngineSound.Stop();
          }
          if(tireScreechSound != null){
            tireScreechSound.Stop();
          }
        }

        if(!useEffects){
          if(RLWParticleSystem != null){
            RLWParticleSystem.Stop();
          }
          if(RRWParticleSystem != null){
            RRWParticleSystem.Stop();
          }
          if(RLWTireSkid != null){
            RLWTireSkid.emitting = false;
          }
          if(RRWTireSkid != null){
            RRWTireSkid.emitting = false;
          }
        }

        var es = EventSystem.current;
        if(es != null){
          var sim = es.GetComponent<StandaloneInputModule>();
          if(sim) sim.enabled = false;
          var isui = es.GetComponent<InputSystemUIInputModule>();
          if(!isui) es.gameObject.AddComponent<InputSystemUIInputModule>();
        }

    }

    void OnEnable(){
      move?.action.Enable();
      handbrakeAction?.action.Enable();
      accelerateAction?.action.Enable();
      reverseAction?.action.Enable();
    }
    void OnDisable(){
      move?.action.Disable();
      handbrakeAction?.action.Disable();
      accelerateAction?.action.Disable();
      reverseAction?.action.Disable();
    }

    // Update is called once per frame
    void Update()
    {

      //CAR DATA

      // We determine the speed of the car.
      carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
      // Save the local velocity of the car in the x axis. Used to know if the car is drifting.
      localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
      // Save the local velocity of the car in the z axis. Used to know if the car is going forward or backwards.
      localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

      //FÍSICA DO CARRO

      /*
      A próxima parte diz respeito ao controlador do carro. Primeiro, verificamos o tipo de entrada (WASD, joystick, botões).

      Os métodos a seguir são chamados conforme a entrada. Por exemplo, chamamos GoForward() quando há aceleração.

      Aqui especificamos o que o carro faz ao acelerar, ré, virar à esquerda/direita e usar o freio de mão.
      */
      {

        bool usedInputSystem = false;
        if(move != null && move.action != null){
          usedInputSystem = true;
          Vector2 m = move.action.ReadValue<Vector2>();
          float x = m.x;
          float y = m.y;
          if(Mathf.Abs(x) < joystickSteerDeadzone) x = 0f;
          bool buttonThrottleAssigned = (accelerateAction != null && accelerateAction.action != null) || (reverseAction != null && reverseAction.action != null);

          if(!buttonThrottleAssigned){
            if(y > 0.1f){
              CancelInvoke("DecelerateCar");
              deceleratingCar = false;
              GoForward();
            }
            if(y < -0.1f){
              CancelInvoke("DecelerateCar");
              deceleratingCar = false;
              GoReverse();
            }
          }

          if(x < -0.1f){
            TurnLeft();
          }
          if(x > 0.1f){
            TurnRight();
          }
          if(Mathf.Abs(x) <= 0.1f && steeringAxis != 0f){
            ResetSteeringAngle();
          }

          float hb = handbrakeAction != null && handbrakeAction.action != null ? handbrakeAction.action.ReadValue<float>() : 0f;
          if(hb > handbrakeButtonThreshold){
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
            Handbrake();
          }else{
            RecoverTraction();
          }

          if(buttonThrottleAssigned){
            float acc = accelerateAction != null && accelerateAction.action != null ? accelerateAction.action.ReadValue<float>() : 0f;
            float rev = reverseAction != null && reverseAction.action != null ? reverseAction.action.ReadValue<float>() : 0f;
            if(acc > accelerateButtonThreshold){
              CancelInvoke("DecelerateCar");
              deceleratingCar = false;
              GoForward();
            }else if(rev > reverseButtonThreshold){
              CancelInvoke("DecelerateCar");
              deceleratingCar = false;
              GoReverse();
            }else{
              ThrottleOff();
              if(decelerateOnNoThrottle && !deceleratingCar){
                InvokeRepeating("DecelerateCar", 0f, 0.1f);
                deceleratingCar = true;
              }
            }
          }
        }
        if(!usedInputSystem){ }

      }


      // We call the method AnimateWheelMeshes() in order to match the wheel collider movements with the 3D meshes of the wheels.
      AnimateWheelMeshes();

    }

    // Converte a velocidade do carro (float) para string e atualiza o texto de UI carSpeedText com este valor.
    public void CarSpeedUI(){

      if(useUI){
          try{
            float absoluteCarSpeed = Mathf.Abs(carSpeed);
            carSpeedText.text = Mathf.RoundToInt(absoluteCarSpeed).ToString();
          }catch(Exception ex){
            Debug.LogWarning(ex);
          }
      }

    }

    // Controla os sons do carro. O motor soa mais lento em baixa velocidade (pitch menor) e mais rápido em alta velocidade
    // (pitch maior: pitch inicial + velocidade/25). Além disso, o chiado do pneu toca quando há derrapagem ou perda de tração.
    public void CarSounds(){

      if(useSounds){
        try{
          if(carEngineSound != null && carEngineSound.isActiveAndEnabled){
            float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.linearVelocity.magnitude) / 25f);
            carEngineSound.pitch = Mathf.Clamp(engineSoundPitch, 0.5f, 3f);
          }
          bool shouldScreech = (isDrifting) || (isTractionLocked && Mathf.Abs(carSpeed) > 12f);
          if(tireScreechSound != null && tireScreechSound.isActiveAndEnabled){
            if(shouldScreech){
              if(!tireScreechSound.isPlaying){
                tireScreechSound.Play();
              }
            }else{
              if(tireScreechSound.isPlaying){
                tireScreechSound.Stop();
              }
            }
          }
        }catch(Exception ex){
          Debug.LogWarning(ex);
        }
      }else if(!useSounds){
        if(carEngineSound != null && carEngineSound.isActiveAndEnabled && carEngineSound.isPlaying){
          carEngineSound.Stop();
        }
        if(tireScreechSound != null && tireScreechSound.isActiveAndEnabled && tireScreechSound.isPlaying){
          tireScreechSound.Stop();
        }
      }

    }

    //
    //MÉTODOS DE DIREÇÃO
    //

    //Vira as rodas dianteiras para a esquerda. A velocidade desse movimento depende de steeringSpeed.
    public void TurnLeft(){
      steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
      if(steeringAxis < -1f){
        steeringAxis = -1f;
      }
      var speedFactor = Mathf.Lerp(1f, 0.6f, Mathf.Clamp01(Mathf.Abs(localVelocityZ) / Mathf.Max(1f, maxSpeed)));
      var steeringAngle = steeringAxis * maxSteeringAngle * speedFactor;
      frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
      frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    //Vira as rodas dianteiras para a direita. A velocidade desse movimento depende de steeringSpeed.
    public void TurnRight(){
      steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
      if(steeringAxis > 1f){
        steeringAxis = 1f;
      }
      var speedFactor = Mathf.Lerp(1f, 0.6f, Mathf.Clamp01(Mathf.Abs(localVelocityZ) / Mathf.Max(1f, maxSpeed)));
      var steeringAngle = steeringAxis * maxSteeringAngle * speedFactor;
      frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
      frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    //Retorna as rodas dianteiras à posição padrão (rotação = 0). A velocidade desse movimento depende de steeringSpeed.
    public void ResetSteeringAngle(){
      if(steeringAxis < 0f){
        steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
      }else if(steeringAxis > 0f){
        steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
      }
      if(Mathf.Abs(frontLeftCollider.steerAngle) < 1f){
        steeringAxis = 0f;
      }
      var speedFactor = Mathf.Lerp(1f, 0.6f, Mathf.Clamp01(Mathf.Abs(localVelocityZ) / Mathf.Max(1f, maxSpeed)));
      var steeringAngle = steeringAxis * maxSteeringAngle * speedFactor;
      frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
      frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    // Sincroniza posição e rotação dos WheelColliders com as malhas 3D das rodas (WheelMeshes).
    void AnimateWheelMeshes(){
      try{
        Quaternion FLWRotation;
        Vector3 FLWPosition;
        frontLeftCollider.GetWorldPose(out FLWPosition, out FLWRotation);
        frontLeftMesh.transform.position = FLWPosition;
        frontLeftMesh.transform.rotation = FLWRotation;

        Quaternion FRWRotation;
        Vector3 FRWPosition;
        frontRightCollider.GetWorldPose(out FRWPosition, out FRWRotation);
        frontRightMesh.transform.position = FRWPosition;
        frontRightMesh.transform.rotation = FRWRotation;

        Quaternion RLWRotation;
        Vector3 RLWPosition;
        rearLeftCollider.GetWorldPose(out RLWPosition, out RLWRotation);
        rearLeftMesh.transform.position = RLWPosition;
        rearLeftMesh.transform.rotation = RLWRotation;

        Quaternion RRWRotation;
        Vector3 RRWPosition;
        rearRightCollider.GetWorldPose(out RRWPosition, out RRWRotation);
        rearRightMesh.transform.position = RRWPosition;
        rearRightMesh.transform.rotation = RRWRotation;
      }catch(Exception ex){
        Debug.LogWarning(ex);
      }
    }

    //
    //MÉTODOS DE MOTOR E FREIO
    //

    // Aplica torque positivo às rodas para ir para frente.
    public void GoForward(){
      //Se as forças aplicadas ao rigidbody no eixo 'x' forem maiores que 2.5f, indica perda de tração; emitir partículas.
      if(Mathf.Abs(localVelocityX) > 2.5f){
        isDrifting = true;
        DriftCarPS();
      }else{
        isDrifting = false;
        DriftCarPS();
      }
      // Ajusta a potência de aceleração até 1 de forma suave.
      throttleAxis = throttleAxis + (Time.deltaTime * 3f);
      if(throttleAxis > 1f){
        throttleAxis = 1f;
      }
      //Se o carro estiver indo para trás, aplicar freio para evitar comportamentos estranhos.
      //Se a velocidade local em 'z' for menor que -1f, é seguro aplicar torque positivo para ir para frente.
      if(localVelocityZ < -1f){
        Brakes();
      }else{
        if(Mathf.RoundToInt(carSpeed) < maxSpeed){
          //Aplicar torque positivo em todas as rodas para ir para frente se maxSpeed não foi atingida.
          frontLeftCollider.brakeTorque = 0;
          frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
          frontRightCollider.brakeTorque = 0;
          frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
          rearLeftCollider.brakeTorque = 0;
          rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
          rearRightCollider.brakeTorque = 0;
          rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
        }else {
          // Se a maxSpeed foi atingida, parar de aplicar torque nas rodas.
          // IMPORTANTE: maxSpeed é uma aproximação; a velocidade real pode ser um pouco maior.
    			frontLeftCollider.motorTorque = 0;
    			frontRightCollider.motorTorque = 0;
          rearLeftCollider.motorTorque = 0;
    			rearRightCollider.motorTorque = 0;
    		}
      }
    }

    // Aplica torque negativo às rodas para ir para trás (ré).
    public void GoReverse(){
      //If the forces aplied to the rigidbody in the 'x' asis are greater than
      //3f, it means that the car is losing traction, then the car will start emitting particle systems.
      if(Mathf.Abs(localVelocityX) > 2.5f){
        isDrifting = true;
        DriftCarPS();
      }else{
        isDrifting = false;
        DriftCarPS();
      }
      // Ajusta a potência de aceleração até -1 de forma suave.
      throttleAxis = throttleAxis - (Time.deltaTime * 3f);
      if(throttleAxis < -1f){
        throttleAxis = -1f;
      }
      //Se o carro ainda estiver indo para frente, aplicar freio para evitar comportamentos estranhos.
      //Se a velocidade local em 'z' for maior que 1f, é seguro aplicar torque negativo para ré.
      if(localVelocityZ > 1f){
        Brakes();
      }else{
        if(Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed){
          //Aplicar torque negativo em todas as rodas para ir em ré se maxReverseSpeed não foi atingida.
          frontLeftCollider.brakeTorque = 0;
          frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
          frontRightCollider.brakeTorque = 0;
          frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
          rearLeftCollider.brakeTorque = 0;
          rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
          rearRightCollider.brakeTorque = 0;
          rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
        }else {
          // Se maxReverseSpeed foi atingida, parar de aplicar torque nas rodas.
          // IMPORTANTE: maxReverseSpeed é uma aproximação; a velocidade real pode ser um pouco maior.
    			frontLeftCollider.motorTorque = 0;
    			frontRightCollider.motorTorque = 0;
          rearLeftCollider.motorTorque = 0;
    			rearRightCollider.motorTorque = 0;
    		}
      }
    }

    //Define o torque do motor como 0 (quando não está acelerando nem dando ré).
    public void ThrottleOff(){
      frontLeftCollider.motorTorque = 0;
      frontRightCollider.motorTorque = 0;
      rearLeftCollider.motorTorque = 0;
      rearRightCollider.motorTorque = 0;
    }

    // Desacelera a velocidade do carro de acordo com decelerationMultiplier (1 = mais lenta, 10 = mais rápida).
    // Método chamado por InvokeRepeating, normalmente a cada 0.1f quando não há aceleração, ré ou freio de mão.
    public void DecelerateCar(){
      if(Mathf.Abs(localVelocityX) > 2.5f){
        isDrifting = true;
        DriftCarPS();
      }else{
        isDrifting = false;
        DriftCarPS();
      }
      // Redefine a potência de aceleração para 0 de forma suave.
      if(throttleAxis != 0f){
        if(throttleAxis > 0f){
          throttleAxis = throttleAxis - (Time.deltaTime * 10f);
        }else if(throttleAxis < 0f){
            throttleAxis = throttleAxis + (Time.deltaTime * 10f);
        }
        if(Mathf.Abs(throttleAxis) < 0.15f){
          throttleAxis = 0f;
        }
      }
      carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.025f * decelerationMultiplier)));
      // Como queremos desacelerar, removemos o torque das rodas.
      frontLeftCollider.motorTorque = 0;
      frontRightCollider.motorTorque = 0;
      rearLeftCollider.motorTorque = 0;
      rearRightCollider.motorTorque = 0;
        // Se a magnitude da velocidade do carro for menor que 0.25f (velocidade muito baixa), pare o carro completamente
        // e cancele a invocação deste método.
      if(carRigidbody.linearVelocity.magnitude < 0.25f){
        carRigidbody.linearVelocity = Vector3.zero;
        CancelInvoke("DecelerateCar");
      }
    }

    // Aplica torque de freio às rodas conforme o brakeForce.
    public void Brakes(){
      frontLeftCollider.brakeTorque = brakeForce;
      frontRightCollider.brakeTorque = brakeForce;
      rearLeftCollider.brakeTorque = brakeForce;
      rearRightCollider.brakeTorque = brakeForce;
    }

    // Faz o carro perder tração, iniciando a derrapagem. A perda de tração depende de handbrakeDriftMultiplier.
    // Valores menores derrapam pouco; valores altos podem fazer parecer que está no gelo.
    public void Handbrake(){
      CancelInvoke("RecoverTraction");
      // Vamos perder tração gradualmente; é onde 'driftingAxis' atua. Esta variável vai de 0 até 1
      // (máxima derrapagem), aumentando suavemente com Time.deltaTime.
      driftingAxis = driftingAxis + (Time.deltaTime);
      float secureStartingPoint = driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;

      if(secureStartingPoint < FLWextremumSlip){
        driftingAxis = FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier);
      }
      if(driftingAxis > 1f){
        driftingAxis = 1f;
      }
      //Se as forças aplicadas ao rigidbody no eixo 'x' forem maiores que 2.5f, indica perda de tração; emitir partículas.
      if(Mathf.Abs(localVelocityX) > 2.5f){
        isDrifting = true;
      }else{
        isDrifting = false;
      }
      //Se 'driftingAxis' não for 1f, as rodas não atingiram a derrapagem máxima; continuar aumentando a
      //fricção lateral das rodas até 'driftingAxis' = 1f.
      if(driftingAxis < 1f){
        FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        frontLeftCollider.sidewaysFriction = FLwheelFriction;

        FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        frontRightCollider.sidewaysFriction = FRwheelFriction;

        RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        rearLeftCollider.sidewaysFriction = RLwheelFriction;

        RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        rearRightCollider.sidewaysFriction = RRwheelFriction;
      }

      // Ao usar o freio de mão, as rodas travam; definimos 'isTractionLocked = true' e o carro passa a emitir trilhas
      // para simular marcas de derrapagem.
      isTractionLocked = true;
      DriftCarPS();

    }

    // Emite tanto as partículas de fumaça dos pneus quanto as trilhas de derrapagem dependendo de 'isDrifting' e 'isTractionLocked'.
    public void DriftCarPS(){

      if(useEffects){
        try{
          if(isDrifting){
            RLWParticleSystem.Play();
            RRWParticleSystem.Play();
          }else if(!isDrifting){
            RLWParticleSystem.Stop();
            RRWParticleSystem.Stop();
          }
        }catch(Exception ex){
          Debug.LogWarning(ex);
        }

        try{
          if((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f){ // Exibe trilhas quando travado ou derrapando e velocidade > 12
            RLWTireSkid.emitting = true;
            RRWTireSkid.emitting = true;
          }else {
            RLWTireSkid.emitting = false;
            RRWTireSkid.emitting = false;
          }
        }catch(Exception ex){
          Debug.LogWarning(ex);
        }
      }else if(!useEffects){
        if(RLWParticleSystem != null){
          RLWParticleSystem.Stop();
        }
        if(RRWParticleSystem != null){
          RRWParticleSystem.Stop();
        }
        if(RLWTireSkid != null){
          RLWTireSkid.emitting = false;
        }
        if(RRWTireSkid != null){
          RRWTireSkid.emitting = false;
        }
      }

    }

    // Recupera a tração do carro quando o usuário para de usar o freio de mão.
    public void RecoverTraction(){
      isTractionLocked = false;
      driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
      if(driftingAxis < 0f){
        driftingAxis = 0f;
      }

      //Se 'driftingAxis' não for 0f, as rodas não recuperaram totalmente a tração.
      //Continuar reduzindo a fricção lateral até alcançar a aderência inicial do carro.
      if(FLwheelFriction.extremumSlip > FLWextremumSlip){
        FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        frontLeftCollider.sidewaysFriction = FLwheelFriction;

        FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        frontRightCollider.sidewaysFriction = FRwheelFriction;

        RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        rearLeftCollider.sidewaysFriction = RLwheelFriction;

        RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
        rearRightCollider.sidewaysFriction = RRwheelFriction;

        Invoke("RecoverTraction", Time.deltaTime);

      }else if (FLwheelFriction.extremumSlip < FLWextremumSlip){
        FLwheelFriction.extremumSlip = FLWextremumSlip;
        frontLeftCollider.sidewaysFriction = FLwheelFriction;

        FRwheelFriction.extremumSlip = FRWextremumSlip;
        frontRightCollider.sidewaysFriction = FRwheelFriction;

        RLwheelFriction.extremumSlip = RLWextremumSlip;
        rearLeftCollider.sidewaysFriction = RLwheelFriction;

        RRwheelFriction.extremumSlip = RRWextremumSlip;
        rearRightCollider.sidewaysFriction = RRwheelFriction;

        driftingAxis = 0f;
      }
    }

}
