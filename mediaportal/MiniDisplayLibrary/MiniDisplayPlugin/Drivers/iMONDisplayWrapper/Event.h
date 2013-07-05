//------------------------------------------------------------------------------
#ifndef EVENT_H_INCLUDED
#define EVENT_H_INCLUDED
//------------------------------------------------------------------------------
#include <windows.h>
//------------------------------------------------------------------------------
class Event
{
public:
  Event();
  virtual ~Event();

private:
  Event(const Event& other) {}
  Event& operator=(const Event& other) { return *this; }

public:
  void Signal() const;
  void Reset() const;
  void Await() const;

private:
  HANDLE m_hEvent;
};
//------------------------------------------------------------------------------
#endif
//------------------------------------------------------------------------------
