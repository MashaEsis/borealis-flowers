window.borealisLs = {
  get: function (k) { return localStorage.getItem(k); },
  set: function (k, v) { localStorage.setItem(k, v); },
  remove: function (k) { localStorage.removeItem(k); }
};
