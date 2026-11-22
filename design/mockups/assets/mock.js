/* AutoRent mockups: just enough behaviour to feel the pages.
   No backend. Everything here is mock data and resets on reload. */
(function () {
  'use strict';

  var EXTRAS = {
    'Child seat': { price: 15, per: 'day' },
    'Roof box': { price: 25, per: 'day' },
    'Additional driver': { price: 50, per: 'once' },
    'Prepaid full tank': { price: 300, per: 'once' }
  };

  function lei(n) { return n.toLocaleString('en-US') + ' lei'; }
  function days(n) { return n + (n === 1 ? ' day' : ' days'); }
  function labelText(label) {
    return Array.prototype.filter.call(label.childNodes, function (n) { return n.nodeType === 3; })
      .map(function (n) { return n.textContent; }).join('').trim();
  }
  function extraCost(name, dayCount) {
    var e = EXTRAS[name];
    return e.per === 'day' ? e.price * dayCount : e.price;
  }
  function el(tag, cls, html) {
    var node = document.createElement(tag);
    if (cls) node.className = cls;
    if (html != null) node.innerHTML = html;
    return node;
  }

  /* ================= Catalog: filter, search, sort ================= */
  var grid = document.querySelector('.catalog .grid');
  if (grid) {
    var MODELS = {
      'Dacia Sandero':       { type: 'City car',      gear: ['Manual', 'Automatic'], engine: ['1.0 TCe 90 HP'], has: ['Navigation', 'Parking sensors'], free: 2 },
      'Dacia Logan':         { type: 'Family saloon', gear: ['Manual', 'Automatic'], engine: ['1.0 TCe 90 HP', '1.5 dCi 115 HP'], has: ['Heated seats', 'Navigation', 'Parking sensors', 'Rear camera'], free: 3 },
      'Skoda Fabia':         { type: 'City car',      gear: ['Manual'],              engine: ['1.5 TSI 150 HP'], has: ['Parking sensors', 'Rear camera'], free: 2 },
      'Renault Clio':        { type: 'City car',      gear: ['Manual', 'Automatic'], engine: ['1.0 TCe 90 HP'], has: ['Heated seats', 'Navigation', 'Parking sensors', 'Rear camera', 'Adaptive cruise control'], free: 1 },
      'Dacia Jogger':        { type: 'Estate',        gear: ['Manual'],              engine: ['1.0 TCe 90 HP'], has: ['Navigation', 'Parking sensors'], free: 3 },
      'Dacia Duster':        { type: 'SUV',           gear: ['Manual', 'Automatic'], engine: ['1.0 TCe 90 HP', '1.5 dCi 115 HP'], has: ['Heated seats', 'Navigation', 'Parking sensors', 'Rear camera'], free: 0 },
      'Renault Captur':      { type: 'SUV',           gear: ['Automatic'],           engine: ['1.0 TCe 90 HP'], has: ['Heated seats', 'Navigation', 'Rear camera'], free: 2 },
      'Volkswagen Golf 8':   { type: 'City car',      gear: ['Manual', 'Automatic'], engine: ['1.5 TSI 150 HP', '2.0 TDI 150 HP'], has: ['Heated seats', 'Navigation', 'Panoramic roof', 'Parking sensors', 'Rear camera', 'Adaptive cruise control'], free: 1 },
      'Skoda Octavia Combi': { type: 'Estate',        gear: ['Manual', 'Automatic'], engine: ['1.5 TSI 150 HP', '2.0 TDI 150 HP'], has: ['Heated seats', 'Navigation', 'Parking sensors', 'Rear camera', 'Adaptive cruise control'], free: 2 }
    };

    var cards = Array.prototype.map.call(grid.querySelectorAll('.model'), function (node) {
      var name = node.querySelector('h3').textContent.trim();
      var m = MODELS[name];
      return {
        node: node, name: name, brand: name.split(' ')[0], type: m.type, gear: m.gear, engine: m.engine, has: m.has, free: m.free,
        price: parseInt(node.querySelector('.model__price b').textContent, 10),
        rating: parseFloat(node.querySelector('.stars').parentNode.textContent.replace('★', ''))
      };
    });

    var form = document.querySelector('.filters form');
    var search = document.querySelector('.catalog__tools input');
    var sort = document.querySelector('.catalog__tools select');
    var applied = document.querySelector('.applied');
    var summary = document.querySelector('.filters > summary');
    var empty = el('div', 'slip slip--pad', '<h3>No model matches these filters</h3><p class="muted" style="margin:6px 0 14px">Try fewer features or a wider price range.</p>');
    var emptyBtn = el('button', 'btn', 'Clear all filters');
    emptyBtn.type = 'button';
    empty.appendChild(emptyBtn);
    empty.hidden = true;
    grid.after(empty);

    function group(legend) {
      var set = Array.prototype.find.call(form.querySelectorAll('fieldset'), function (f) {
        return f.querySelector('legend').textContent.trim() === legend;
      });
      return Array.prototype.map.call(set.querySelectorAll('label.check'), function (label) {
        return { input: label.querySelector('input'), text: labelText(label) };
      });
    }
    function picked(legend) {
      return group(legend).filter(function (o) { return o.input.checked && o.text !== 'Any'; });
    }

    function applyFilters() {
      var types = picked('Type'), brands = picked('Brand'), gear = picked('Gearbox'), engine = picked('Engine');
      var wants = picked('Comfort').concat(picked('Driver assistance'));
      var onlyFree = picked('Availability').length > 0;
      var range = form.querySelectorAll('.range input');
      var min = parseInt(range[0].value, 10) || 0, max = parseInt(range[1].value, 10) || Infinity;
      var q = search.value.trim().toLowerCase();
      var names = function (list) { return list.map(function (o) { return o.text; }); };

      var shown = cards.filter(function (c) {
        var ok = (!types.length || names(types).indexOf(c.type) > -1)
          && (!brands.length || names(brands).indexOf(c.brand) > -1)
          && (!gear.length || c.gear.indexOf(gear[0].text) > -1)
          && (!engine.length || c.engine.indexOf(engine[0].text) > -1)
          && wants.every(function (w) { return c.has.indexOf(w.text) > -1; })
          && (!onlyFree || c.free > 0)
          && c.price >= min && c.price <= max
          && (!q || c.name.toLowerCase().indexOf(q) > -1);
        c.node.hidden = !ok;
        return ok;
      });

      var by = sort.value;
      shown.sort(function (a, b) {
        if (by === 'Price: high to low') return b.price - a.price;
        if (by === 'Best rated') return b.rating - a.rating;
        if (by === 'Name A to Z') return a.name.localeCompare(b.name);
        return a.price - b.price;
      }).forEach(function (c) { grid.appendChild(c.node); });

      // Pills for everything applied; clicking one removes that filter
      var active = types.concat(brands, gear, engine, wants, onlyFree ? picked('Availability') : []);
      applied.innerHTML = '';
      applied.appendChild(el('span', 'muted', shown.length + ' of ' + cards.length + ' models on this page' + (active.length || q ? ' match' : '')));
      active.forEach(function (o) {
        var pill = el('button', 'pill', o.text === 'Only models with a car free for my dates' ? 'Free for my dates' : o.text);
        pill.type = 'button';
        pill.addEventListener('click', function () {
          if (o.input.type === 'radio') form.querySelector('input[name="' + o.input.name + '"]').checked = true;
          else o.input.checked = false;
          applyFilters();
        });
        applied.appendChild(pill);
      });
      summary.textContent = active.length ? 'Filters (' + active.length + ' applied)' : 'Filters';
      empty.hidden = shown.length > 0;
      grid.hidden = shown.length === 0;
    }

    function clearAll() {
      form.querySelectorAll('input[type="checkbox"]').forEach(function (i) { i.checked = false; });
      form.querySelector('input[name="gear"]').checked = true;
      form.querySelector('input[name="engine"]').checked = true;
      var range = form.querySelectorAll('.range input');
      range[0].value = 100; range[1].value = 400;
      search.value = '';
      applyFilters();
    }

    form.addEventListener('input', applyFilters);
    search.addEventListener('input', applyFilters);
    sort.addEventListener('change', applyFilters);
    form.querySelector('button').addEventListener('click', clearAll);
    emptyBtn.addEventListener('click', clearAll);
    applyFilters();
  }

  /* ================= Car page: dates, extras, total ================= */
  var book = document.querySelector('.book');
  if (book) {
    var RATE = 223;
    // Days in December 2026 when this car is already rented: [from, to)
    var BUSY = [[new Date(2026, 11, 7, 10), new Date(2026, 11, 10, 10)], [new Date(2026, 11, 17, 10), new Date(2026, 11, 21, 10)]];
    var from = book.querySelector('#from'), to = book.querySelector('#to');
    var lengthNote = book.querySelector('.book__dates + .fine');
    var lines = book.querySelector('.lines');
    var addBtn = book.querySelector('.btn--primary');
    var stamp = document.querySelector('.cal').parentNode.querySelector('.stamp');
    var req = document.querySelector('.cal__req');
    var dayLabels = document.querySelectorAll('.cal__days span');

    function recalc() {
      var a = new Date(from.value), b = new Date(to.value);
      var valid = !isNaN(a) && !isNaN(b) && b > a;
      var n = valid ? Math.ceil((b - a) / 864e5) : 0;
      var clash = valid && BUSY.some(function (r) { return a < r[1] && b > r[0]; });
      var total = n * RATE;

      lines.innerHTML = '';
      if (valid) {
        lines.appendChild(el('div', '', '<dt>Car, ' + days(n) + ' × ' + RATE + ' lei</dt><dd>' + lei(n * RATE) + '</dd>'));
        book.querySelectorAll('label.check').forEach(function (label) {
          if (!label.querySelector('input').checked) return;
          var name = labelText(label), cost = extraCost(name, n);
          total += cost;
          var what = EXTRAS[name].per === 'day' ? name + ', ' + days(n) + ' × ' + EXTRAS[name].price + ' lei' : name;
          lines.appendChild(el('div', '', '<dt>' + what + '</dt><dd>' + lei(cost) + '</dd>'));
        });
      }
      lines.appendChild(el('div', 'total', '<dt>Total for this car</dt><dd>' + (valid ? lei(total) : '–') + '</dd>'));

      lengthNote.textContent = valid ? days(n) + '. A started day counts as a full day.' : 'The return must be after the pick-up.';
      lengthNote.style.color = valid ? '' : 'var(--stamp-red)';

      var ok = valid && !clash;
      stamp.className = 'stamp ' + (ok ? 'stamp--ok' : 'stamp--no');
      stamp.textContent = !valid ? 'Check your dates' : clash ? 'Already rented on these dates' : 'Free for your dates';
      addBtn.setAttribute('aria-disabled', String(!ok));
      addBtn.style.opacity = ok ? '' : '0.4';
      addBtn.style.pointerEvents = ok ? '' : 'none';
      addBtn.textContent = ok ? 'Add to cart' : 'Pick other dates to add this car';

      // Move the highlighted interval on the two-week strip (7 to 20 December)
      var first = new Date(2026, 11, 7), start = 0, end = 0;
      if (valid) {
        start = Math.max(1, Math.floor((a - first) / 864e5) + 1);
        end = Math.min(15, Math.floor((new Date(b - 1) - first) / 864e5) + 2);
      }
      var visible = valid && start < end && start <= 14 && end >= 2;
      req.hidden = !visible;
      if (visible) req.style.gridColumn = start + ' / ' + end;
      dayLabels.forEach(function (s, i) { s.classList.toggle('req', visible && i + 1 >= start && i + 1 < end); });
    }

    book.addEventListener('input', recalc);
    recalc();

    // Photo thumbnails
    var main = document.querySelector('.photo img');
    document.querySelectorAll('.thumbs button').forEach(function (button) {
      button.addEventListener('click', function () {
        main.src = button.querySelector('img').src;
        document.querySelectorAll('.thumbs button').forEach(function (b) { b.setAttribute('aria-pressed', String(b === button)); });
      });
    });
  }

  /* ================= Cart: extras, remove, summary ================= */
  var cart = document.querySelector('.cart');
  if (cart) {
    var list = cart.firstElementChild;
    var hooks = document.querySelector('.hooks');
    var intro = document.querySelector('.page h1 + p');
    var sumLines = cart.querySelector('.sum .lines');
    var checkout = cart.querySelector('.sum .btn--primary');
    var sumStamp = cart.querySelector('.sum .stamp');
    var emptyHook = cart.querySelector('.empty-hook');
    var headerKeys = document.querySelectorAll('.cartlink .keys i');

    function refresh() {
      var items = Array.prototype.slice.call(list.querySelectorAll('.item'));
      var grand = 0;
      sumLines.innerHTML = '';

      items.forEach(function (item) {
        var n = +item.dataset.days, rate = +item.dataset.rate, total = n * rate, anyExtra = false;
        item.querySelectorAll('label.check').forEach(function (label) {
          var name = labelText(label), e = EXTRAS[name], on = label.querySelector('input').checked;
          if (on) { total += extraCost(name, n); anyExtra = true; }
          label.querySelector('.price').textContent = on ? lei(extraCost(name, n)) : e.price + ' lei ' + (e.per === 'day' ? 'a day' : 'once');
        });
        item.querySelector('.item__foot .muted').textContent = days(n) + ' × ' + rate + ' lei' + (anyExtra ? ' + extras' : '');
        item.querySelector('.item__foot b').textContent = lei(total);
        sumLines.appendChild(el('div', '', '<dt>' + item.dataset.name + ', ' + item.dataset.plate + '</dt><dd>' + lei(total) + '</dd>'));
        grand += total;
      });

      sumLines.appendChild(el('div', '', '<dt>Pick-up place</dt><dd class="muted">chosen at checkout</dd>'));
      sumLines.appendChild(el('div', 'total', '<dt>Total so far</dt><dd>' + lei(grand) + '</dd>'));

      // Three hooks: one key per car, the rest are free
      hooks.innerHTML = '';
      for (var i = 0; i < 3; i++) {
        var slot = el('div');
        slot.appendChild(el('span', items[i] ? 'tag' : 'tag tag--out', items[i] ? items[i].dataset.plate : 'Free hook'));
        hooks.appendChild(slot);
        if (headerKeys[i]) headerKeys[i].className = items[i] ? '' : 'out';
      }

      var left = 3 - items.length;
      intro.textContent = items.length === 0 ? 'No cars yet. All three hooks are free.'
        : (items.length === 1 ? '1 car' : items.length + ' cars') + '. One order can hold up to 3, so there ' + (left === 1 ? 'is one hook' : 'are ' + left + ' hooks') + ' left.';
      emptyHook.firstElementChild.innerHTML = items.length === 0
        ? '<b>Your cart is empty.</b> <span class="muted">Pick a car and its key hangs here.</span>'
        : '<b>' + (left === 1 ? 'One hook left.' : left + ' hooks left.') + '</b> <span class="muted">Need a second car for the grandparents?</span>';
      emptyHook.querySelector('.btn').textContent = items.length === 0 ? 'Browse cars' : 'Add another car';

      var has = items.length > 0;
      checkout.style.opacity = has ? '' : '0.4';
      checkout.style.pointerEvents = has ? '' : 'none';
      checkout.setAttribute('aria-disabled', String(!has));
      sumStamp.hidden = !has;
      sumStamp.textContent = items.length === 1 ? 'This car is free' : 'Both cars are free';
    }

    list.addEventListener('change', refresh);
    list.addEventListener('click', function (event) {
      var button = event.target.closest('button');
      if (!button || button.textContent.trim() !== 'Remove') return;
      button.closest('.item').remove();
      refresh();
    });
    refresh();
  }

  /* ================= Round 2: shared small behaviours ================= */

  // Lists that can be narrowed by tabs and a search box.
  // <div class="tabs" data-tabs="#list">, <input data-search="#list">, rows carry data-status.
  function applyList(target) {
    var id = '#' + target.id;
    var bar = document.querySelector('[data-tabs="' + id + '"]');
    var box = document.querySelector('[data-search="' + id + '"]');
    var want = bar ? bar.querySelector('[aria-pressed="true"]').dataset.value : 'all';
    var q = box ? box.value.trim().toLowerCase() : '';
    var shown = 0;
    target.querySelectorAll('[data-status]').forEach(function (row) {
      var ok = (want === 'all' || row.dataset.status.split(' ').indexOf(want) > -1) && (!q || row.closest('thead') || row.textContent.toLowerCase().indexOf(q) > -1);
      row.hidden = !ok;
      if (ok && !row.closest('thead')) shown++;
    });
    var none = document.querySelector('[data-empty="' + id + '"]');
    if (none) none.hidden = shown > 0;
  }
  document.querySelectorAll('[data-tabs]').forEach(function (bar) {
    var target = document.querySelector(bar.dataset.tabs);
    bar.addEventListener('click', function (event) {
      var button = event.target.closest('button');
      if (!button) return;
      bar.querySelectorAll('button').forEach(function (b) { b.setAttribute('aria-pressed', String(b === button)); });
      applyList(target);
    });
    applyList(target);
  });
  document.querySelectorAll('[data-search]').forEach(function (box) {
    box.addEventListener('input', function () { applyList(document.querySelector(box.dataset.search)); });
  });

  // Forms: data-go sends you on, data-done shows a confirmation in place.
  document.querySelectorAll('form[data-go], form[data-done]').forEach(function (formEl) {
    formEl.addEventListener('submit', function (event) {
      event.preventDefault();
      if (formEl.dataset.go) { window.location.href = formEl.dataset.go; return; }
      var note = formEl.querySelector('.notice') || el('p', 'notice');
      note.textContent = formEl.dataset.done;
      note.setAttribute('role', 'status');
      formEl.prepend(note);
      note.scrollIntoView({ block: 'nearest' });
    });
  });

  // Home: length of the rental under the date fields
  var heroForm = document.querySelector('.hero__form');
  if (heroForm) {
    var hf = heroForm.querySelectorAll('input'), hint = heroForm.querySelector('p');
    var showLength = function () {
      var a = new Date(hf[0].value), b = new Date(hf[1].value);
      hint.textContent = b > a ? days(Math.ceil((b - a) / 864e5)) + '. A started day counts as a full day.' : 'The return must be after the pick-up.';
    };
    heroForm.addEventListener('input', showLength);
    showLength();
  }

  // Model page: hide the keys that are out
  var onlyFree = document.querySelector('#only-free');
  if (onlyFree) onlyFree.addEventListener('change', function () {
    document.querySelectorAll('.hook.is-out').forEach(function (h) { h.hidden = onlyFree.checked; });
  });

  // Checkout: the pick-up place sets the fee and the total
  var pickups = document.querySelectorAll('input[name="pickup"]');
  if (pickups.length) {
    var SUB = 1504, feeOut = document.querySelector('#fee'), totalOut = document.querySelector('#total'), where = document.querySelector('#deliver-to');
    var setFee = function () {
      var chosen = document.querySelector('input[name="pickup"]:checked'), fee = +chosen.dataset.fee;
      feeOut.textContent = fee ? lei(fee) : 'free';
      totalOut.textContent = lei(SUB + fee);
      where.hidden = chosen.value !== 'delivery';
      where.querySelector('input').required = chosen.value === 'delivery';
    };
    pickups.forEach(function (r) { r.addEventListener('change', setFee); });
    setFee();
  }

  // Order placed: pretend the card payment went through
  var pay = document.querySelector('#pay');
  if (pay) pay.addEventListener('click', function () {
    document.querySelector('#pay-state').className = 'stamp stamp--ok';
    document.querySelector('#pay-state').textContent = 'Paid';
    document.querySelector('#pay-title').textContent = 'Paid. Your keys are waiting.';
    document.querySelector('#pay-text').textContent = 'Both cars are reserved for you. Bring your driving licence and this order number to the pick-up.';
    document.querySelectorAll('[data-after-pay]').forEach(function (n) { n.hidden = false; });
    pay.hidden = true;
  });

  // Saved cars: remove one
  var saved = document.querySelector('#saved');
  if (saved) saved.addEventListener('click', function (event) {
    var button = event.target.closest('button[data-remove]');
    if (!button) return;
    button.closest('.model').remove();
    var left = saved.querySelectorAll('.model').length;
    document.querySelector('#saved-count').textContent = left === 1 ? '1 car saved.' : left + ' cars saved.';
    document.querySelector('#saved-empty').hidden = left > 0;
  });

  // Admin orders: changing the status updates the row
  var STATUS_CHIP = { 'new': 'chip chip--wait', 'paid': 'chip chip--paid', 'handed': 'chip chip--road', 'returned': 'chip', 'cancelled': 'chip chip--no' };
  document.querySelectorAll('select[data-order-status]').forEach(function (sel) {
    sel.addEventListener('change', function () {
      var row = sel.closest('tr'), chip = row.querySelector('.chip');
      row.dataset.status = sel.value;
      chip.className = STATUS_CHIP[sel.value];
      chip.textContent = sel.options[sel.selectedIndex].text;
    });
  });

  // Photo thumbnails on pages without the booking panel
  if (!document.querySelector('.book')) {
    document.querySelectorAll('.thumbs button').forEach(function (button) {
      button.addEventListener('click', function () {
        document.querySelector('.photo img').src = button.querySelector('img').src;
        document.querySelectorAll('.thumbs button').forEach(function (b) { b.setAttribute('aria-pressed', String(b === button)); });
      });
    });
  }
})();
