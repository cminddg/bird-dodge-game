(() => {
  "use strict";

  const VW = 1280;
  const VH = 720;
  const GROUND_Y = 642;
  const PIPE_WIDTH = 86;
  const PIPE_GAP = 220;
  const START_X = 300;
  const BIRD_HEIGHT = 78;

  const canvas = document.getElementById("gameCanvas");
  const ctx = canvas.getContext("2d");
  const livesText = document.getElementById("livesText");
  const scoreText = document.getElementById("scoreText");
  const bestText = document.getElementById("bestText");
  const overlay = document.getElementById("overlay");
  const overlayEyebrow = document.getElementById("overlayEyebrow");
  const overlayTitle = document.getElementById("overlayTitle");
  const overlayCopy = document.getElementById("overlayCopy");
  const primaryButton = document.getElementById("primaryButton");

  const music = new Audio("./assets/audio/owies-ukulele.mp3");
  const flapSound = new Audio("./assets/audio/light-button.mp3");
  music.loop = true;
  music.volume = 0.2;
  flapSound.volume = 0.45;

  const birdSources = [1, 2, 3].map((bird) =>
    [1, 2, 3, 4].map((frame) => `./assets/birds/bird${bird}/Frame${frame}.png`)
  );

  const birds = birdSources.map((frames) =>
    frames.map((src) => {
      const image = new Image();
      image.src = src;
      return image;
    })
  );

  const state = {
    mode: "ready",
    scale: 1,
    offsetX: 0,
    offsetY: 0,
    lastTime: performance.now(),
    elapsed: 0,
    score: 0,
    best: Number(localStorage.getItem("bird-dodge-best") || 0),
    livesLost: 0,
    frameIndex: 0,
    invulnerable: 0,
    spawnTimer: 0,
    pipes: [],
    clouds: [],
  };

  const player = {
    x: START_X,
    y: 330,
    vy: 0,
    rotation: 0,
  };

  function resize() {
    const dpr = Math.max(1, Math.min(window.devicePixelRatio || 1, 2));
    canvas.width = Math.floor(window.innerWidth * dpr);
    canvas.height = Math.floor(window.innerHeight * dpr);
    canvas.style.width = `${window.innerWidth}px`;
    canvas.style.height = `${window.innerHeight}px`;
    state.scale = Math.min(window.innerWidth / VW, window.innerHeight / VH);
    state.offsetX = (window.innerWidth - VW * state.scale) / 2;
    state.offsetY = (window.innerHeight - VH * state.scale) / 2;
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  }

  function resetRun() {
    state.mode = "ready";
    state.elapsed = 0;
    state.score = 0;
    state.livesLost = 0;
    state.frameIndex = 0;
    state.invulnerable = 0;
    state.spawnTimer = 0;
    state.pipes = [];
    state.clouds = makeClouds();
    player.x = START_X;
    player.y = 330;
    player.vy = 0;
    player.rotation = 0;
    updateHud();
    showReady();
  }

  function startRun() {
    state.mode = "running";
    state.lastTime = performance.now();
    overlay.classList.add("is-hidden");
    if (player.vy === 0) {
      player.vy = -390;
    }
    playMusic();
  }

  function gameOver() {
    state.mode = "gameover";
    music.pause();
    if (state.score > state.best) {
      state.best = state.score;
      localStorage.setItem("bird-dodge-best", String(state.best));
    }
    updateHud();
    overlay.classList.remove("is-hidden");
    overlayEyebrow.textContent = "GAME OVER";
    overlayTitle.textContent = `分數 ${state.score}`;
    overlayCopy.textContent = `Best Score ${state.best}。按 Restart 再飛一次，不用再賭 R 鍵有沒有醒著。`;
    primaryButton.textContent = "Restart";
  }

  function showReady() {
    overlay.classList.remove("is-hidden");
    overlayEyebrow.textContent = "BIRD DODGE";
    overlayTitle.textContent = "準備起飛";
    overlayCopy.textContent = "按 Start 開始。空白鍵或滑鼠點擊會讓小鳥上升，並切換目前這隻鳥的造型。";
    primaryButton.textContent = "Start";
  }

  function togglePause() {
    if (state.mode === "running") {
      state.mode = "paused";
      music.pause();
      overlay.classList.remove("is-hidden");
      overlayEyebrow.textContent = "PAUSED";
      overlayTitle.textContent = "暫停中";
      overlayCopy.textContent = "按 Start 或 Space 繼續。網頁不能像桌面程式一樣用 Esc 關掉，但至少不用叫工作管理員了。";
      primaryButton.textContent = "Start";
    } else if (state.mode === "paused") {
      startRun();
    }
  }

  function updateHud() {
    livesText.textContent = `${Math.max(0, 3 - state.livesLost)}/3`;
    scoreText.textContent = String(state.score);
    bestText.textContent = `BEST ${Math.max(state.best, state.score)}`;
  }

  function playMusic() {
    music.play().catch(() => {
      // Browsers only allow audio after a user gesture; the next click or space will retry.
    });
  }

  function playFlapSound() {
    flapSound.currentTime = 0;
    flapSound.play().catch(() => {});
  }

  function flap() {
    if (state.mode === "ready") {
      startRun();
    } else if (state.mode === "paused") {
      startRun();
      return;
    } else if (state.mode === "gameover") {
      resetRun();
      startRun();
      return;
    }

    if (state.mode !== "running") {
      return;
    }

    player.vy = -430;
    state.frameIndex = (state.frameIndex + 1) % birds[state.livesLost].length;
    playFlapSound();
    playMusic();
  }

  function loseLife() {
    if (state.invulnerable > 0 || state.mode !== "running") {
      return;
    }

    state.livesLost += 1;
    state.frameIndex = 0;
    state.invulnerable = 1.1;
    player.vy = -330;
    player.y = Math.max(145, Math.min(player.y, GROUND_Y - 140));

    if (state.livesLost >= 3) {
      gameOver();
      return;
    }

    updateHud();
  }

  function addScore(amount) {
    state.score += amount;
    updateHud();
  }

  function difficulty() {
    const stage = Math.floor(state.elapsed / 15);
    return {
      speed: 235 + stage * 28,
      spawnEvery: Math.max(1.05, 1.55 - stage * 0.08),
      gap: Math.max(178, PIPE_GAP - stage * 8),
    };
  }

  function spawnPipe() {
    const { gap } = difficulty();
    const topLimit = 112;
    const bottomLimit = GROUND_Y - 118;
    const center = topLimit + gap / 2 + Math.random() * (bottomLimit - topLimit - gap);
    state.pipes.push({
      x: VW + 80,
      gapY: center,
      gap,
      scored: false,
    });
  }

  function update(dt) {
    if (state.mode !== "running") {
      return;
    }

    state.elapsed += dt;
    state.invulnerable = Math.max(0, state.invulnerable - dt);

    const { speed, spawnEvery } = difficulty();
    state.spawnTimer -= dt;
    if (state.spawnTimer <= 0) {
      spawnPipe();
      state.spawnTimer = spawnEvery;
    }

    player.vy += 980 * dt;
    player.y += player.vy * dt;
    player.rotation = Math.max(-0.5, Math.min(0.75, player.vy / 720));

    if (player.y > GROUND_Y - BIRD_HEIGHT / 2 || player.y < 36) {
      loseLife();
    }

    state.pipes.forEach((pipe) => {
      pipe.x -= speed * dt;
      if (!pipe.scored && pipe.x + PIPE_WIDTH < player.x) {
        pipe.scored = true;
        addScore(10);
      }
      if (hitsPipe(pipe)) {
        loseLife();
      }
    });

    state.pipes = state.pipes.filter((pipe) => pipe.x > -PIPE_WIDTH - 40);
    updateClouds(dt, speed);
  }

  function currentBirdSize() {
    const image = birds[Math.min(state.livesLost, 2)][state.frameIndex];
    const aspect = image.naturalWidth && image.naturalHeight ? image.naturalWidth / image.naturalHeight : 1;
    return { width: BIRD_HEIGHT * aspect, height: BIRD_HEIGHT };
  }

  function playerBounds() {
    const size = currentBirdSize();
    return {
      left: player.x - size.width * 0.34,
      right: player.x + size.width * 0.34,
      top: player.y - size.height * 0.34,
      bottom: player.y + size.height * 0.35,
    };
  }

  function hitsPipe(pipe) {
    const body = playerBounds();
    const inPipeX = body.right > pipe.x && body.left < pipe.x + PIPE_WIDTH;
    if (!inPipeX) {
      return false;
    }
    return body.top < pipe.gapY - pipe.gap / 2 || body.bottom > pipe.gapY + pipe.gap / 2;
  }

  function makeClouds() {
    return [
      { x: 160, y: 150, size: 1.05, speed: 18 },
      { x: 610, y: 108, size: 0.72, speed: 14 },
      { x: 920, y: 240, size: 0.98, speed: 20 },
    ];
  }

  function updateClouds(dt, speed) {
    state.clouds.forEach((cloud) => {
      cloud.x -= (cloud.speed + speed * 0.03) * dt;
      if (cloud.x < -170) {
        cloud.x = VW + 150 + Math.random() * 180;
        cloud.y = 90 + Math.random() * 190;
      }
    });
  }

  function clearScreen() {
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = "#82c7e9";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    const dpr = Math.max(1, Math.min(window.devicePixelRatio || 1, 2));
    ctx.setTransform(state.scale * dpr, 0, 0, state.scale * dpr, state.offsetX * dpr, state.offsetY * dpr);
  }

  function render() {
    clearScreen();
    drawBackground();
    drawPipes();
    drawBird();
    drawForeground();
  }

  function drawBackground() {
    const sky = ctx.createLinearGradient(0, 0, 0, GROUND_Y);
    sky.addColorStop(0, "#7fc6eb");
    sky.addColorStop(1, "#afe5f4");
    ctx.fillStyle = sky;
    ctx.fillRect(0, 0, VW, GROUND_Y);

    ctx.fillStyle = "#71b665";
    ctx.fillRect(0, GROUND_Y, VW, VH - GROUND_Y);
    ctx.fillStyle = "#337e39";
    ctx.fillRect(0, GROUND_Y + 26, VW, VH - GROUND_Y - 26);
    ctx.fillStyle = "#cde96b";
    ctx.fillRect(0, GROUND_Y - 8, VW, 14);

    state.clouds.forEach(drawCloud);
  }

  function drawCloud(cloud) {
    ctx.save();
    ctx.translate(cloud.x, cloud.y);
    ctx.scale(cloud.size, cloud.size);
    ctx.globalAlpha = 0.82;
    ctx.fillStyle = "#ffffff";
    ctx.beginPath();
    ctx.ellipse(-58, 12, 56, 30, 0, 0, Math.PI * 2);
    ctx.ellipse(5, -4, 52, 44, 0, 0, Math.PI * 2);
    ctx.ellipse(64, 14, 48, 32, 0, 0, Math.PI * 2);
    ctx.ellipse(8, 28, 78, 25, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();
  }

  function drawPipes() {
    state.pipes.forEach((pipe) => {
      const topBottom = pipe.gapY - pipe.gap / 2;
      const bottomTop = pipe.gapY + pipe.gap / 2;
      drawPipe(pipe.x, 0, PIPE_WIDTH, topBottom, true);
      drawPipe(pipe.x, bottomTop, PIPE_WIDTH, GROUND_Y - bottomTop + 10, false);
    });
  }

  function drawPipe(x, y, width, height, isTop) {
    if (height <= 0) {
      return;
    }
    ctx.fillStyle = "#248e51";
    ctx.fillRect(x, y, width, height);
    ctx.fillStyle = "#176a3f";
    ctx.fillRect(x + width - 18, y, 18, height);
    ctx.fillStyle = "#f0d74a";
    const lipY = isTop ? y + height - 18 : y;
    ctx.fillRect(x - 14, lipY, width + 28, 24);
  }

  function drawBird() {
    const birdIndex = Math.min(state.livesLost, 2);
    const image = birds[birdIndex][state.frameIndex];
    const size = currentBirdSize();
    const blink = state.invulnerable > 0 && Math.floor(state.invulnerable * 14) % 2 === 0;

    if (blink) {
      return;
    }

    ctx.save();
    ctx.translate(player.x, player.y);
    ctx.rotate(player.rotation);
    if (image.complete && image.naturalWidth > 0) {
      ctx.drawImage(image, -size.width / 2, -size.height / 2, size.width, size.height);
    } else {
      ctx.fillStyle = "#6ed5db";
      ctx.beginPath();
      ctx.ellipse(0, 0, size.width / 2, size.height / 2, 0, 0, Math.PI * 2);
      ctx.fill();
    }
    ctx.restore();
  }

  function drawForeground() {
    ctx.fillStyle = "rgba(38, 91, 62, 0.16)";
    ctx.fillRect(0, GROUND_Y - 2, VW, 2);
  }

  function loop(now) {
    const dt = Math.min(0.033, (now - state.lastTime) / 1000 || 0);
    state.lastTime = now;
    update(dt);
    render();
    requestAnimationFrame(loop);
  }

  primaryButton.addEventListener("click", () => {
    if (state.mode === "gameover") {
      resetRun();
    }
    startRun();
  });

  window.addEventListener("keydown", (event) => {
    if (event.code === "Space") {
      event.preventDefault();
      flap();
    } else if (event.code === "KeyR") {
      resetRun();
      startRun();
    } else if (event.code === "Escape") {
      togglePause();
    }
  });

  window.addEventListener("pointerdown", (event) => {
    if (event.target === primaryButton) {
      return;
    }
    flap();
  });

  window.addEventListener("resize", resize);

  resize();
  resetRun();
  requestAnimationFrame(loop);
})();
